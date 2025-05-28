using SSO.Services.DTOs;

namespace SSO.Services.Interfaces;

public interface IUserService
{
    public Task<List<string>> Login(ProviderDetails details);

    public Task<TokenDTO> GetTokens(TokenDTO tokenDTO, string audience, string issuer);
    
}