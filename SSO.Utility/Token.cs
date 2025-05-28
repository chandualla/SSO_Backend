using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography;

namespace SSO.Utility.HelperClasses
{
    public class TokenUtility
    {
        public static string GenerateJwtToken(string tokenType, string issuer, string audience, string ssoId, string email, string privateKeyPem)
        {
            var rsa = RSA.Create();
            rsa.ImportFromPem(privateKeyPem.ToCharArray());
            var securityKey = new RsaSecurityKey(rsa);
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.RsaSha256);
            var tokenHandler = new JwtSecurityTokenHandler();
            var claimsList = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, email),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim(JwtRegisteredClaimNames.Iss, issuer),
                new Claim(JwtRegisteredClaimNames.Aud, audience),
                new Claim(JwtRegisteredClaimNames.Sid, ssoId)
            };

            var expiration = tokenType == "Access"
                ? DateTime.UtcNow.AddMinutes(30)
                : DateTime.UtcNow.AddMinutes(1440);

            var exp = new DateTimeOffset(expiration).ToUnixTimeSeconds().ToString();
            claimsList.Add(new Claim(JwtRegisteredClaimNames.Exp, exp));

            var jwt = new JwtSecurityToken(
                claims: claimsList,
                signingCredentials: credentials
            );
            string ssoJwt = tokenHandler.WriteToken(jwt);

            return ssoJwt;
        }
    }
}