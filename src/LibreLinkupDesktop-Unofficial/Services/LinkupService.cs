using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using LibreLinkupDesktopUnofficial.Models;

namespace LibreLinkupDesktopUnofficial.Services;

public class LinkupService
{
    private const string BaseUrlTemplate = "https://api-COUNTRY_CODE.libreview.io/llu";
    private readonly HttpClient _httpClient;
    private readonly LogService _log;

    public LinkupService(LogService logService)
    {
        _log = logService;
        var handler = new SocketsHttpHandler
        {
            AutomaticDecompression = System.Net.DecompressionMethods.All,
            PooledConnectionLifetime = TimeSpan.FromMinutes(5)
        };
        _httpClient = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(30) };
        _httpClient.DefaultRequestHeaders.ExpectContinue = false;
        _httpClient.DefaultRequestHeaders.ConnectionClose = false;
        _httpClient.DefaultRequestHeaders.Add("Accept", "*/*");
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "FetchAPI-User-Agent");
    }

    private static string GetBaseUrl(string countryCode)
    {
        if (string.Equals(countryCode, "global", StringComparison.OrdinalIgnoreCase))
        {
            return BaseUrlTemplate.Replace("-COUNTRY_CODE", "");
        }
        return BaseUrlTemplate.Replace("COUNTRY_CODE", countryCode.ToLower());
    }

    private static string Hash256(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    public async Task<(LoginResult? Result, string? Error)> GetAuthTokenAsync(
        string country, string username, string password, CancellationToken ct = default)
    {
        try
        {
            var baseUrl = GetBaseUrl(country);
            _log.Log("=== LOGIN ATTEMPT ===");
            _log.Log($"Base URL: {baseUrl}/auth/login");

            var loginPayload = JsonSerializer.Serialize(new { email = username, password });
            var (responseData, _) = await PostJsonAsync($"{baseUrl}/auth/login", loginPayload, ct);

            if (responseData == null)
            {
                return (null, "Login failed. No response from server.");
            }

            var respElement = responseData.Value;
            _log.LogObject("Response Data", respElement);

            if (respElement.TryGetProperty("status", out var statusProp) && statusProp.GetInt32() == 0)
            {
                // Determine account country from response
                string countryCode = country;
                if (respElement.TryGetProperty("data", out var dataProp))
                {
                    if (dataProp.TryGetProperty("user", out var userProp) &&
                        userProp.TryGetProperty("country", out var countryProp))
                    {
                        countryCode = countryProp.GetString() ?? country;
                    }
                    else if (dataProp.TryGetProperty("region", out var regionProp))
                    {
                        countryCode = regionProp.GetString() ?? country;
                    }
                }

                if (string.Equals(countryCode, "ch", StringComparison.OrdinalIgnoreCase))
                    countryCode = "eu";

                _log.Log($"Country from response: {countryCode}");

                // Retry with correct country URL if needed
                baseUrl = GetBaseUrl(countryCode);
                if (!string.Equals(countryCode, country, StringComparison.OrdinalIgnoreCase))
                {
                    _log.Log($"Country mismatch, retrying with: {countryCode}");
                }

                var (retryData, _) = await PostJsonAsync($"{baseUrl}/auth/login", loginPayload, ct);
                if (retryData == null)
                {
                    return (null, "Login failed on retry.");
                }

                _log.LogObject("Retry Response", retryData);

                var retryElement = retryData.Value;
                if (retryElement.TryGetProperty("data", out var retryDataProp))
                {
                    var token = retryDataProp.GetProperty("authTicket").GetProperty("token").GetString();
                    var accountId = retryDataProp.GetProperty("user").GetProperty("id").GetString();
                    var finalCountry = retryDataProp.GetProperty("user").GetProperty("country").GetString()?.ToLower();
                    if (string.Equals(finalCountry, "ch", StringComparison.OrdinalIgnoreCase))
                        finalCountry = "eu";

                    _log.Log($"Login successful for country: {finalCountry}");
                    return (new LoginResult
                    {
                        Token = token ?? "",
                        AccountId = accountId ?? "",
                        AccountCountry = finalCountry ?? countryCode
                    }, null);
                }
            }
            else
            {
                int status = 999999;
                if (respElement.TryGetProperty("status", out var errStatusProp) &&
                    errStatusProp.ValueKind == JsonValueKind.Number)
                {
                    status = errStatusProp.GetInt32();
                }
                _log.Log($"Login failed with status: {status}");
                return (null, "Login failed. Please check your credentials.");
            }
        }
        catch (OperationCanceledException)
        {
            _log.Log("Login timed out");
            return (null, "Login timed out.");
        }
        catch (Exception ex)
        {
            _log.LogError($"Unable to get the token: {ex.Message}");
            return (null, ex.Message);
        }

        return (null, "Login failed. Unexpected response.");
    }

    public async Task<(CgmConnectionData? Data, string? Error)> GetCgmDataAsync(
        string token, string country, string accountId, CancellationToken ct = default)
    {
        try
        {
            var baseUrl = GetBaseUrl(country);
            var accountIdHash = Hash256(accountId);

            _log.Log($"Fetching connections from: {baseUrl}/connections");

            var connResponse = await GetJsonAsync($"{baseUrl}/connections", token, accountIdHash, ct);
            if (connResponse == null)
            {
                return (null, "Failed to fetch connections.");
            }

            _log.LogObject("Connections response", connResponse);

            string? patientId = null;
            if (connResponse.Value.TryGetProperty("data", out var dataProp) &&
                dataProp.ValueKind == JsonValueKind.Array)
            {
                if (dataProp.GetArrayLength() == 0)
                {
                    return (null, "No LibreLinkUp connections found. Please set up a connection in your Libre app.");
                }

                var firstConn = dataProp[0];
                if (firstConn.TryGetProperty("patientId", out var pidProp))
                {
                    patientId = pidProp.GetString();
                }
            }

            if (string.IsNullOrEmpty(patientId))
            {
                return (null, "No patient ID found.");
            }

            _log.Log($"Fetching graph data for patient: {patientId}");

            var graphResponse = await GetJsonAsync($"{baseUrl}/connections/{patientId}/graph", token, accountIdHash, ct);
            if (graphResponse == null)
            {
                return (null, "Failed to fetch CGM data.");
            }

            _log.Log("CGM data received successfully");

            if (graphResponse.Value.TryGetProperty("data", out var graphDataProp) &&
                graphDataProp.TryGetProperty("connection", out var connectionProp))
            {
                var result = new CgmConnectionData();

                if (connectionProp.TryGetProperty("glucoseMeasurement", out var gmProp))
                {
                    result.GlucoseMeasurement = new GlucoseMeasurement
                    {
                        Value = gmProp.TryGetProperty("Value", out var valProp)
                            ? valProp.GetDouble()
                            : gmProp.TryGetProperty("ValueInMgPerDl", out var mgProp) ? mgProp.GetDouble() : 0,
                        ValueInMgPerDl = gmProp.TryGetProperty("ValueInMgPerDl", out _),
                        TrendArrow = gmProp.TryGetProperty("TrendArrow", out var taProp) ? taProp.GetInt32() : 0,
                        Timestamp = gmProp.TryGetProperty("Timestamp", out var tsProp) ? tsProp.GetString() ?? "" : ""
                    };
                }

                if (connectionProp.TryGetProperty("firstName", out var fnProp))
                    result.FirstName = fnProp.GetString() ?? "";
                if (connectionProp.TryGetProperty("lastName", out var lnProp))
                    result.LastName = lnProp.GetString() ?? "";

                return (result, null);
            }

            return (null, "Unexpected response format.");
        }
        catch (OperationCanceledException)
        {
            return (null, "Request timed out.");
        }
        catch (Exception ex)
        {
            _log.LogError($"Unable to getCGMData: {ex.Message}");
            return (null, ex.Message);
        }
    }

    private async Task<(JsonElement? Data, int StatusCode)> PostJsonAsync(
        string url, string jsonPayload, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Content = new StringContent(jsonPayload, Encoding.UTF8);
        request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
        request.Headers.Add("product", "llu.android");
        request.Headers.Add("version", "4.16.0");
        request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = true };
        request.Headers.Pragma.Add(new NameValueHeaderValue("no-cache"));

        var response = await _httpClient.SendAsync(request, ct);
        _log.Log($"Response Status: {(int)response.StatusCode}");

        var body = await response.Content.ReadAsStringAsync(ct);
        if (string.IsNullOrWhiteSpace(body) || body.TrimStart().StartsWith('<'))
        {
            _log.LogError($"Non-JSON response (status {(int)response.StatusCode}): {body.Substring(0, Math.Min(200, body.Length))}");
            return (null, (int)response.StatusCode);
        }
        var doc = JsonDocument.Parse(body);
        return (doc.RootElement.Clone(), (int)response.StatusCode);
    }

    private async Task<JsonElement?> GetJsonAsync(
        string url, string token, string accountIdHash, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("product", "llu.android");
        request.Headers.Add("version", "4.16.0");
        request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = true };
        request.Headers.Pragma.Add(new NameValueHeaderValue("no-cache"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.Add("Account-Id", accountIdHash);

        var response = await _httpClient.SendAsync(request, ct);
        var body = await response.Content.ReadAsStringAsync(ct);
        if (string.IsNullOrWhiteSpace(body) || body.TrimStart().StartsWith('<'))
        {
            _log.LogError($"Non-JSON response (status {(int)response.StatusCode}): {body.Substring(0, Math.Min(200, body.Length))}");
            return null;
        }
        var doc = JsonDocument.Parse(body);
        return doc.RootElement.Clone();
    }
}
