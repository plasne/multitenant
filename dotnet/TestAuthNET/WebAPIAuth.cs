using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Web;
using System.Web.Http;

namespace TestAuthNET
{

    public class Config
    {

        private Dictionary<string, string> items = new Dictionary<string, string>();

        public string GetValue(string key)
        {
            if (!items.ContainsKey(key))
            {
                string value = ConfigurationManager.AppSettings[key];
                items.Add(key, value);
            }
            return items[key];
        }

        public string GetEncodedValue(string key)
        {
            string value = GetValue(key);
            return HttpContext.Current.Server.UrlEncode(value);
        }

    }

    public class WebAPIAuth : ApiController
    {

        private string GetKey()
        {
            TripleDESCryptoServiceProvider crypto = new TripleDESCryptoServiceProvider();
            crypto.GenerateIV();
            crypto.GenerateKey();
            byte[] key = crypto.Key;
            return Convert.ToBase64String(key);
        }

        private string ConsentUri(HttpResponseMessage response, string add = "")
        {

            // generate an authstate key so that you can ensure the consent through token process is continuous
            string authstate = GetKey();
            var cookie = new CookieHeaderValue("authstate", authstate);
            cookie.Expires = DateTimeOffset.Now.AddMinutes(5);
            cookie.Domain = Request.RequestUri.Host;
            cookie.Path = "/";
            response.Headers.AddCookies(new CookieHeaderValue[] { cookie });

            // generate the URL
            HttpServerUtility s = HttpContext.Current.Server;
            Config c = new Config();
            string url = c.GetValue("authority") + "/oauth2/authorize?response_type=code&client_id=" + c.GetEncodedValue("clientId") + "&redirect_uri=" + c.GetEncodedValue("redirectUri") + "&state=" + s.UrlEncode(authstate) + "&resource=" + c.GetEncodedValue("resource") + add;
            return url;

        }

        [HttpGet, Route("consent")]
        public HttpResponseMessage Consent()
        {
            HttpResponseMessage response = new HttpResponseMessage(System.Net.HttpStatusCode.Redirect);
            string url = ConsentUri(response, "&prompt=admin_consent");
            response.Headers.Location = new Uri(url);
            return response;
        }

        [HttpGet, Route("login")]
        public HttpResponseMessage Login()
        {
            HttpResponseMessage response = new HttpResponseMessage(System.Net.HttpStatusCode.Redirect);
            string url = ConsentUri(response);
            response.Headers.Location = new Uri(url);
            return response;
        }

        private AuthenticationResult GetAccessTokenFromCode(string code)
        {
            Config c = new Config();
            AuthenticationContext context = new AuthenticationContext(c.GetValue("authority"));
            AuthenticationResult result = context.AcquireTokenByAuthorizationCode(code,
                new Uri(c.GetValue("redirectUri")),
                new ClientCredential(c.GetValue("clientId"), c.GetValue("clientSecret")),
                c.GetValue("resource"));
            return result;
        }

        private class GroupMembership
        {
            public List<string> value;
        }

        private GroupMembership GetGroupMembershipForUser(HttpResponseMessage response, string token, string domain, string user)
        {
            try
            {
                WebClient client = new WebClient();
                client.Headers.Add("Authorization", "bearer " + token);
                response.Headers.Add("path00", "yes");
                response.Headers.Add("path00a", "https://graph.windows.net/" + domain + "/users/" + user + "/getMemberGroups?api-version=1.6");
                string json = client.UploadString("https://graph.windows.net/" + domain + "/users/" + user + "/getMemberGroups?api-version=1.6",
                    "{ \"securityEnabledOnly\": false }");
                response.Headers.Add("path01", "yes");
                GroupMembership membership = Newtonsoft.Json.JsonConvert.DeserializeObject<GroupMembership>(json);
                response.Headers.Add("path02", "yes");
                response.Headers.Add("path03", membership.value.Count.ToString());
                return membership;
            }
            catch (Exception ex)
            {
                response.Headers.Add("exception", ex.Message);
                throw;
            }
        }

        [HttpGet, Route("token")] //?code={code}&state={state}&session_state={session_state}&admin_consent={admin_consent}
        public HttpResponseMessage Token(string code, string state, string session_state)
        {
            HttpResponseMessage response = new HttpResponseMessage();

            // ensure this is all part of the same authorization chain
            CookieHeaderValue cookie = Request.Headers.GetCookies("authstate").FirstOrDefault();
            if (cookie != null && cookie["authstate"].Value == state)
            {

                // get an access token for the specified resource from the code
                try
                {
                    AuthenticationResult result = GetAccessTokenFromCode(code);

                    // get a list of groups that the user is a member of
                    try
                    {
                        //string user = result.UserInfo.UniqueId;
                        string user = "admin@peterlasne.onmicrosoft.com";
                        string domain = user.Split('@')[1];

                        response.Headers.Add("token", result.AccessToken);
                        response.Headers.Add("user", user);
                        response.Headers.Add("domain", domain);

                        GroupMembership membership = GetGroupMembershipForUser(response, result.AccessToken, domain, user);

                        response.StatusCode = HttpStatusCode.InternalServerError;
                        response.ReasonPhrase = string.Join(", ", membership.value.ToArray());

                    }
                    catch (Exception ex)
                    {
                        response.StatusCode = System.Net.HttpStatusCode.Unauthorized;
                        response.ReasonPhrase = "Unauthorized (membership): " + ex.Message;
                    }

                }
                catch (Exception ex)
                {
                    response.StatusCode = System.Net.HttpStatusCode.Unauthorized;
                    response.ReasonPhrase = "Unauthorized (access token): " + ex.Message;
                }

            }
            else
            {
                response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                response.ReasonPhrase = "Bad Request: this does not appear to be part of the same authorization chain.";
            }

            return response;
        }

    }





}