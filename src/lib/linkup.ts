import { fetch } from '@tauri-apps/plugin-http'
import { hash256Async } from './utils'

const BASE_URL = 'https://api-COUNTRY_CODE.libreview.io/llu'

const getBaseUrl = (countryCode: string): string => {
  if(countryCode === 'global') {
    return BASE_URL.replace('-COUNTRY_CODE', '')
  }
  return BASE_URL.replace('COUNTRY_CODE', countryCode)
}

type LoginAttemptRequest = {
  country: string
  username: string
  password: string
}

type GetGeneralRequest = {
  token: string
  country: string
  accountId: string
}

export async function getAuthToken(request: LoginAttemptRequest): Promise<{
  token: string, accountId: string, accountCountry: string,
} | { error: number } | null> {
  try {
    let baseUrl = getBaseUrl(request.country);
    console.log('=== LOGIN ATTEMPT ===');
    console.log('Base URL:', `${baseUrl}/auth/login`);
    console.log('Login Data:', { email: request.username, password: '***' });

    let response = await fetch(`${baseUrl}/auth/login`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'product': 'llu.android',
        'version': '4.16.0',
        'Pragma': 'no-cache',
        'Cache-Control': 'no-cache',
      },
      body: JSON.stringify({
        email: request.username,
        password: request.password,
      }),
    });

    console.log('Response Status:', response.status);
    let responseData = await response.json();
    console.log('Response Data:', responseData);

    if (responseData?.status === 0 ) {
      // Handle different response structures
      let countryCode;
      if (responseData.data.user?.country) {
        // Original structure: {data: {user: {country: "fr"}}}
        countryCode = responseData.data.user.country;
        // if countryCode is ch, set it to eu
        if (countryCode.toLowerCase() === 'ch') {
          countryCode = 'eu';
        }
      } else if (responseData.data.region) {
        // New structure: {data: {region: "fr"}}
        countryCode = responseData.data.region;
      } else {
        // Fallback to original request country
        countryCode = request.country;
      }

      baseUrl = getBaseUrl(countryCode);
      
      console.log('Country from response:', countryCode);
      if (countryCode !== request.country.toLowerCase()) {
        console.log('🔄 Country mismatch, retrying with:', countryCode);
      }

      response = await fetch(`${baseUrl}/auth/login`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'product': 'llu.android',
          'version': '4.16.0',
          'Pragma': 'no-cache',
          'Cache-Control': 'no-cache',
        },
        body: JSON.stringify({
          email: request.username,
          password: request.password,
        }),
      });
      
      responseData = await response.json();
      console.log('Retry Response:', responseData);
    }
    else{
      console.log('❌ Login failed with status:', responseData?.status);
      return {
        error: responseData?.status || 999999
      };
    }

    let finalCountryCode = responseData?.data?.user?.country?.toLowerCase();
    finalCountryCode = finalCountryCode === 'ch' ? 'eu' : finalCountryCode;

    console.log('✅ Login successful for country:', finalCountryCode);
    return {
      token: responseData?.data?.authTicket?.token,
      accountId: responseData?.data?.user?.id,
      accountCountry: finalCountryCode,
    };

  } catch (error: any) {
    console.log("❌ Unable to get the token:", error.message);
    if (error.response) {
      console.log("Response status:", error.response.status);
      console.log("Response data:", error.response.data);
    }
    throw error;
  }

  return null;
}

export async function getCGMData(request: GetGeneralRequest): Promise<any|null|{error: string, message: string}> {
  try {
    const baseURL = getBaseUrl(request.country)
    const accountIdHash = await hash256Async(request.accountId)
    const headers = {
      product: 'llu.android',
      version: '4.16.0',
      Pragma: 'no-cache',
      'Cache-Control': 'no-cache',
      Authorization: `Bearer ${request.token}`,
      'Account-Id': accountIdHash,
    }

    console.log('Fetching connections from:', baseURL + '/connections');

    const connResponse = await fetch(`${baseURL}/connections`, {
      method: 'GET',
      headers,
    })

    const connData = await connResponse.json();
    console.log('Connections response:', connData);

    const patientId = connData?.data?.[0]?.patientId

    if (!patientId) {
      if (connData?.data?.length === 0) {
        return { error: 'NO_CONNECTIONS', message: 'No LibreLinkUp connections found. Please set up a connection in your Libre app.' }
      }
      return null
    }

    console.log('Fetching graph data for patient:', patientId);

    const graphResponse = await fetch(`${baseURL}/connections/${patientId}/graph`, {
      method: 'GET',
      headers,
    })

    const graphData = await graphResponse.json();
    console.log('✅ Graph data received');
    return graphData?.data?.connection
  } catch (error: any) {
    console.log('❌ Unable to getCGMData:', error.message)
    if (error.response) {
      console.log("Response status:", error.response.status);
      console.log("Response data:", error.response.data);
    }
  }

  return null
}

export async function getConnection(request: GetGeneralRequest): Promise<any|null> {
  try {
    const baseURL = getBaseUrl(request.country)
    const accountIdHash = await hash256Async(request.accountId)
    const headers = {
      product: 'llu.android',
      version: '4.16.0',
      Pragma: 'no-cache',
      'Cache-Control': 'no-cache',
      Authorization: `Bearer ${request.token}`,
      'Account-Id': accountIdHash,
    }

    const response = await fetch(`${baseURL}/connections`, {
      method: 'GET',
      headers,
    })

    const data = await response.json();
    return data?.data?.[0]
  } catch (error: any) {
    console.log('Unable to getConnection:', error.message)
  }

  return null
}
