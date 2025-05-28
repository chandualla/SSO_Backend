namespace SSO.Services.DTOs;

public class ProviderDetails
{
    public string Code { get; set; }

    public string ClientSecret { get; set; }

    public string RedirectUri { get; set; }

    public string ClientId { get; set; }

    public HttpClient Client { get; set; }

    public string Issuer { get; set; }

    public string Audience { get; set; }

    public string Scopes { get; set; }

    public string Provider { get; set; }

    public string TokenURL { get; set; }

    public string EmailClaim { get; set; }
}