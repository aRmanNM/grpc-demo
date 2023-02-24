using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;

namespace server.Services;

public class DemoTokenValidator : ISecurityTokenValidator
{
    public bool CanValidateToken => true;

    public int MaximumTokenSizeInBytes { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public bool CanReadToken(string securityToken) => true;

    public ClaimsPrincipal ValidateToken(string securityToken, TokenValidationParameters validationParameters, out SecurityToken validatedToken)
    {
        validatedToken = new JwtSecurityToken();

        var user = Encoding.UTF8.GetString(Convert.FromBase64String(securityToken));

        if (user == "grpc")
            throw new InvalidDataException();

        var identity = new ClaimsIdentity(
            new Claim[] { new Claim(ClaimTypes.NameIdentifier, user) });

        return new ClaimsPrincipal(identity);
    }
}
