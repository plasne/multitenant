using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Claims;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;

namespace TestAuthNET
{
    [ServiceContract]
    public class WCFService
    {

        [DataContract]
        public class Reply
        {

            [DataMember]
            public List<string> claims;

            [DataMember]
            public string message;

            public Reply()
            {
                claims = new List<string>();
            }
        }

        [WebGet(UriTemplate = "hello", ResponseFormat = WebMessageFormat.Json), OperationContract]
        public Reply Hello()
        {
            try
            {
                Reply reply = new Reply();

                // report on claims
                ClaimsPrincipal principal = WcfOperationContext.Current.Items["principal"] as ClaimsPrincipal;
                foreach (Claim claim in principal.Claims)
                {
                    reply.claims.Add("[" + claim.Type + "] " + claim.Value);
                }

                // write the message
                Claim user = principal.Claims.First(claim => claim.Type.Equals("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", StringComparison.InvariantCultureIgnoreCase));
                reply.message = "Hello from the WCF Service. Happy to see you, " + ((user != null) ? user.Value : "unknown") + ".";

                return reply;
            }
            catch (Exception ex)
            {
                var webContext = new WebOperationContext(OperationContext.Current);
                webContext.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                webContext.OutgoingResponse.Headers.Add("exception", ex.Message);
                return null;
            }
        }

    }
}
