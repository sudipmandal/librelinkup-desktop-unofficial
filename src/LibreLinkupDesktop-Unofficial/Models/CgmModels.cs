namespace LibreLinkupDesktopUnofficial.Models;

public class GlucoseMeasurement
{
    public double Value { get; set; }
    public bool ValueInMgPerDl { get; set; }
    public int TrendArrow { get; set; }
    public string Timestamp { get; set; } = string.Empty;
}

public class CgmConnectionData
{
    public GlucoseMeasurement? GlucoseMeasurement { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}

public class LoginResult
{
    public string Token { get; set; } = string.Empty;
    public string AccountId { get; set; } = string.Empty;
    public string AccountCountry { get; set; } = string.Empty;
}

public class LoginError
{
    public int Error { get; set; }
}

public class CgmError
{
    public string Error { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class CountryOption
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public override string ToString() => Name;
}
