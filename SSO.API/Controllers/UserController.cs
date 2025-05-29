
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SSO.Services.DTOs;
using SSO.Services.Interfaces;
using SSO.Utility.HelperClasses;

[ApiController]
[Route("[controller]/[action]")]
public class UserController : ControllerBase
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    private readonly IHttpClientFactory _httpClientFactory;

    private readonly IConfiguration _configuration;

    private readonly IUserService _userService;

    private readonly IWebHostEnvironment _env;

    public UserController(IHttpContextAccessor httpContextAccessor, IHttpClientFactory httpClientFactory, IConfiguration configuration, IUserService userService, IWebHostEnvironment env)
    {
        _httpContextAccessor = httpContextAccessor;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _userService = userService;
        _env = env;
    }

    [HttpGet]
    public async Task<IActionResult> Login([FromQuery] string code, [FromQuery] string? scope, [FromQuery] string? authuser, [FromQuery] string? prompt, [FromQuery] string state)
    {
        try
        {
            var parsedState = JsonSerializer.Deserialize<Dictionary<string, string>>(state);
            string spRedirect = parsedState["redirect"];
            string provider = parsedState["provider"];
            ProviderDetails details = new ProviderDetails
            {
                Code = code,
                Client = _httpClientFactory.CreateClient(),
                ClientId = _configuration[$"OAuthProviders:{provider}:ClientId"]!,
                RedirectUri = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/User/Login",
                ClientSecret = _configuration[$"OAuthProviders:{provider}:ClientSecret"]!,
                Issuer = _configuration["JwtSettings:Issuer"]!,
                Audience = _configuration["JwtSettings:Audience"]!,
                TokenURL = _configuration[$"OAuthProviders:{provider}:TokenUrl"]!,
                Scopes = scope,
                Provider = provider,
                EmailClaim = _configuration[$"OAuthProviders:{provider}:EmailClaim"]!
            };

            List<string> tokens = await _userService.Login(details);

            TokenDTO tokenDTO = new TokenDTO
            {
                AccessToken = tokens[0],
                RefreshToken = tokens[1]
            };
            string json = System.Text.Json.JsonSerializer.Serialize(tokenDTO);
            string encoded = System.Web.HttpUtility.UrlEncode(json);
            return Redirect($"{spRedirect}?token={encoded}");

        }
        catch (Exception ex)
        {
            return Ok("Error at the controller");
        }
    }

    [HttpPost]
    public async Task<IActionResult> RefreshTokens([FromBody] TokenDTO tokens, [FromQuery] string redirect_uri)
    {
        try
        {
            var issuer = _configuration["JwtSettings:Issuer"];
            var audience = _configuration["JwtSettings:Audience"];
            TokenDTO tokenDTO = await _userService.GetTokens(tokens, audience, issuer);
            return Ok(tokenDTO);
        }
        catch
        {
            var url = _configuration["SSO:ui"];
            return Redirect($"{url}/?redirect=${redirect_uri}");
        }
    }

}