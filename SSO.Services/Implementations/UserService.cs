using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.IdentityModel.Tokens;
using SSO.Repository.Entities;
using SSO.Repository.Interfaces;
using SSO.Services.DTOs;
using SSO.Services.Interfaces;
using SSO.Utility.HelperClasses;

namespace SSO.Services.Implementations;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IWebHostEnvironment _env;
    public UserService(IUserRepository userRepository, IWebHostEnvironment env)
    {
        _userRepository = userRepository;
        _env = env;
    }

    public async Task<List<string>> Login(ProviderDetails details)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(details.Code))
            {
                throw new Exception("Missing code parameter");
            }

            var requestData = new Dictionary<string, string>
            {
                { "code", details.Code },
                { "client_id", details.ClientId },
                { "client_secret", details.ClientSecret },
                { "redirect_uri", details.RedirectUri },
                { "grant_type", "authorization_code" },
                {"scope",details.Scopes}
            };

            var requestContent = new FormUrlEncodedContent(requestData);

            var response = await details.Client.PostAsync(details.TokenURL, requestContent);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception(errorContent);
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonDocument.Parse(responseContent).RootElement;

            if (!tokenResponse.TryGetProperty("access_token", out var accessTokenElement))
            {
                throw new Exception("Access token not found in response");
            }

            string accessToken = accessTokenElement.GetString();

            if (!tokenResponse.TryGetProperty("id_token", out var idTokenElement))
            {
                throw new Exception("ID token not found in response");
            }

            string idToken = idTokenElement.GetString();

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(idToken);
            var email = jwtToken.Claims.First(c => c.Type == details.EmailClaim).Value;

            if (string.IsNullOrWhiteSpace(email))
                throw new Exception($"Email claim '{details.EmailClaim}' not found in token.");

            User user = await _userRepository.GetUser(email);   

            // var privateKeyPath = Environment.GetEnvironmentVariable("PRIVATE_KEY_PEM");
            var privateKeyPem = Environment.GetEnvironmentVariable("PRIVATE_KEY_PEM");
            string access = TokenUtility.GenerateJwtToken("Access", details.Issuer, details.Audience, user.Id.ToString(), email, privateKeyPem);

            string refresh = TokenUtility.GenerateJwtToken("Refresh", details.Issuer, details.Audience, user.Id.ToString(), email, privateKeyPem);

            return [access, refresh];
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    public async Task<TokenDTO> GetTokens(TokenDTO tokenDTO, string audience, string issuer)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var rsa = RSA.Create();
        // var publicKeyPath = Path.Combine(_env.ContentRootPath, "Keys", "public_key.pem");
        var publicKeyPem = Environment.GetEnvironmentVariable("PUBLIC_KEY_PEM");
        rsa.ImportFromPem(publicKeyPem.ToCharArray());
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new RsaSecurityKey(rsa)
        };

        // Try to validate access token (even if expired)
        ClaimsPrincipal principal;
        try
        {
            principal = tokenHandler.ValidateToken(tokenDTO.AccessToken, validationParameters, out SecurityToken validatedToken);

            // Check if token is expired
            var jwtToken = (JwtSecurityToken)validatedToken;
            var exp = jwtToken.Payload.Exp;
            var expiryDate = DateTimeOffset.FromUnixTimeSeconds(exp.Value).UtcDateTime;

            if (expiryDate > DateTime.UtcNow)
            {
                // Access token is still valid, no need to refresh
                return tokenDTO; // optional: or return message saying still valid
            }
        }
        catch
        {
            throw new Exception("Relogin...");
        }
        // Check refresh token validity (very simple check using same logic)
        try
        {
            tokenHandler.ValidateToken(tokenDTO.RefreshToken, validationParameters, out SecurityToken refreshToken);

            var jwtRefresh = (JwtSecurityToken)refreshToken;
            var exp = jwtRefresh.Payload.Exp;
            var expiryDate = DateTimeOffset.FromUnixTimeSeconds(exp.Value).UtcDateTime;

            if (expiryDate < DateTime.UtcNow)
            {
                throw new Exception("Relogin");
            }

            // 🔑 Both tokens valid, but access token expired → issue new tokens
            var email = principal.FindFirst(ClaimTypes.Email)?.Value;
            var ssoId = principal.FindFirst(JwtRegisteredClaimNames.Sid)?.Value;

            var privateKeyPath = Path.Combine(_env.ContentRootPath, "Keys", "private_key.pem");
            var privateKeyPem = File.ReadAllText(privateKeyPath);

            string newAccessToken = TokenUtility.GenerateJwtToken("Access", issuer, audience, ssoId, email, privateKeyPem);
            string newRefreshToken = TokenUtility.GenerateJwtToken("Refresh", issuer, audience, ssoId, email, privateKeyPem);

            var newTokenObj = new TokenDTO
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            };

            return newTokenObj;
        }
        catch
        {
            throw new Exception("Relogin!!!");
        }
    }
}