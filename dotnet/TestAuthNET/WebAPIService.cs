using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Security.Claims;
using System.ServiceModel.Security.Tokens;
using System.Web.Http;

namespace TestAuthNET
{
    public class WebAPIService : ApiController
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

        [Authorize]
        [HttpGet, Route("hello")]
        public Reply Hello()
        {
            Reply reply = new Reply();

            // write out the claims
            ClaimsIdentity identity = User.Identity as ClaimsIdentity;
            foreach (Claim claim in identity.Claims)
            {
                reply.claims.Add("[" + claim.Type + " " + claim.Value);
            }

            // write the message
            Claim user = identity.Claims.First(claim => claim.Type.Equals("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", StringComparison.InvariantCultureIgnoreCase));
            reply.message = "Hello from the WebAPI Service. Happy to see you, " + ((user != null) ? user.Value : "unknown") + ".";

            return reply;
        }



    }
}