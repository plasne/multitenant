using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Security.Tokens;
using System.ServiceModel.Web;
using System.Web;

namespace TestAuthNET
{

    public class WcfOperationContext : IExtension<OperationContext>
    {
        private readonly Dictionary<string, object> items;

        public Dictionary<string, object> Items
        {
            get { return items; }
        }

        public static WcfOperationContext Current
        {
            get
            {
                WcfOperationContext context = OperationContext.Current.Extensions.Find<WcfOperationContext>();
                if (context == null)
                {
                    context = new WcfOperationContext();
                    OperationContext.Current.Extensions.Add(context);
                }
                return context;
            }
        }

        public void Attach(OperationContext owner)
        {
        }

        public void Detach(OperationContext owner)
        {
        }

        private WcfOperationContext()
        {
            items = new Dictionary<string, object>();
        }

    }

    public class WCFAuthorization : System.ServiceModel.ServiceAuthorizationManager
    {

        protected override bool CheckAccessCore(OperationContext operationContext)
        {

            // check for the existance of an authorization header
            string authorization = WebOperationContext.Current.IncomingRequest.Headers["Authorization"];
            if (authorization != null && authorization.Length > 0)
            {
                try
                {

                    // get the secret
                    string key_s = ConfigurationManager.ConnectionStrings["JWTKey"].ConnectionString;
                    byte[] key_b = System.Text.Encoding.UTF8.GetBytes(key_s);

                    // determine what a valid token would look like
                    TokenValidationParameters validationParams =
                        new TokenValidationParameters()
                        {
                            IssuerSigningToken = new BinarySecretSecurityToken(key_b),
                            RequireExpirationTime = true,
                            ValidateIssuer = true,
                            ValidIssuer = "http://testauth.plasne.com",
                            ValidateAudience = false
                        };

                    // validate the token
                    JwtSecurityTokenHandler jwtHandler = new JwtSecurityTokenHandler();
                    SecurityToken validated;
                    string token = authorization.Replace("Bearer ", string.Empty);
                    ClaimsPrincipal principal = jwtHandler.ValidateToken(token, validationParams, out validated);

                    // store the identity
                    WcfOperationContext.Current.Items.Add("principal", principal);
                    return true;

                }
                catch (Exception ex)
                {
                    var webContext = new WebOperationContext(operationContext);
                    webContext.OutgoingResponse.StatusCode = HttpStatusCode.Unauthorized;
                    webContext.OutgoingResponse.Headers.Add("exception", ex.Message);
                    return false;
                }
            }
            else
            {
                var webContext = new WebOperationContext(operationContext);
                webContext.OutgoingResponse.StatusCode = HttpStatusCode.Unauthorized;
                return false;
            }

        }

    }


}