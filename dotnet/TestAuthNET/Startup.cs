using Microsoft.Owin;
using Owin;
using System.Configuration;
using System.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.ServiceModel.Security.Tokens;

[assembly: OwinStartup(typeof(TestAuthNET.Startup))]

namespace TestAuthNET
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {

            // JWT Bearer
            if (ConfigurationManager.ConnectionStrings["JWTKey"] != null)
            {
                string key_s = ConfigurationManager.ConnectionStrings["JWTKey"].ConnectionString;
                byte[] key_b = System.Text.Encoding.UTF8.GetBytes(key_s);
                app.UseJwtBearerAuthentication(
                    new Microsoft.Owin.Security.Jwt.JwtBearerAuthenticationOptions
                    {
                        TokenValidationParameters = new TokenValidationParameters
                        {
                            IssuerSigningToken = new BinarySecretSecurityToken(key_b),
                            RequireExpirationTime = true,
                            ValidateIssuer = true,
                            ValidIssuer = "http://testauth.plasne.com",
                            ValidateAudience = false
                        }
                    });
            }

        }



    }
}
