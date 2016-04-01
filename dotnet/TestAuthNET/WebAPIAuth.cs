using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
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
            CookieHeaderValue cookie = new CookieHeaderValue("authstate", authstate);
            cookie.Expires = DateTimeOffset.Now.AddMinutes(5);
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
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.Redirect);
            string url = ConsentUri(response, "&prompt=admin_consent");
            response.Headers.Location = new Uri(url);
            return response;
        }

        [HttpGet, Route("login")]
        public HttpResponseMessage Login()
        {
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.Redirect);
            string url = ConsentUri(response);
            response.Headers.Location = new Uri(url);
            return response;
        }

        private AuthenticationResult GetAccessTokenFromCode(string code)
        {
            Config c = new Config();
            Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationContext context =
                new Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationContext(c.GetValue("authority"));
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

        private List<string> GetGroupMembershipForUser(string token, string domain, string user)
        {
            WebClient client = new WebClient();
            client.Headers.Add("Authorization", "bearer " + token);
            client.Headers.Add("Content-Type", "application/json");
            string json = client.UploadString("https://graph.windows.net/" + domain + "/users/" + user + "/getMemberGroups?api-version=1.6",
                "{ \"securityEnabledOnly\": false }");
            GroupMembership membership = Newtonsoft.Json.JsonConvert.DeserializeObject<GroupMembership>(json);
            return membership.value;
        }

        private class GroupDetailsIn
        {
            public List<string> objectIds;
            public List<string> types = new List<string> { "group" };
        }

        private class Group
        {
            public string displayName;
        }

        private class GroupDetailsOut
        {
            public List<Group> value;
        }
        
        private List<Group> GetGroupDetails(string token, string domain, List<string> groups)
        {
            WebClient client = new WebClient();
            client.Headers.Add("Authorization", "bearer " + token);
            client.Headers.Add("Content-Type", "application/json");
            GroupDetailsIn groupDetailsIn = new GroupDetailsIn() { objectIds = groups };
            string json_in = Newtonsoft.Json.JsonConvert.SerializeObject(groupDetailsIn);
            string json_out = client.UploadString("https://graph.windows.net/" + domain + "/getObjectsByObjectIds?api-version=1.6", json_in);
            GroupDetailsOut details = Newtonsoft.Json.JsonConvert.DeserializeObject<GroupDetailsOut>(json_out);
            return details.value;
        }

        [HttpGet, Route("token")] //?code={code}&state={state}&session_state={session_state}&admin_consent={admin_consent}
        public HttpResponseMessage Token(string code, string state, string session_state)
        {
            HttpResponseMessage response = new HttpResponseMessage();

            // ensure this is all part of the same authorization chain
            CookieHeaderValue authstate_cookie = Request.Headers.GetCookies("authstate").FirstOrDefault();
            if (authstate_cookie != null && authstate_cookie["authstate"].Value == state)
            {

                // get an access token for the specified resource from the code
                try
                {
                    AuthenticationResult result = GetAccessTokenFromCode(code);

                    // get a list of groups that the user is a member of
                    try
                    {
                        //string user = result.UserInfo.UniqueId;
                        string user = result.UserInfo.DisplayableId;
                        string domain = user.Split('@')[1];
                        List<string> groups = GetGroupMembershipForUser(result.AccessToken, domain, user);

                        // get the group details (really just need the display name)
                        try
                        {
                            List<Group> details = GetGroupDetails(result.AccessToken, domain, groups);

                            List<string> membership = new List<string>();
                            details.ForEach(group => membership.Add(group.displayName.Replace("testauth_", "")));
                            string scope_json = Newtonsoft.Json.JsonConvert.SerializeObject(membership);

                            List<string> rights = new List<string>();
                            if (membership.Contains("admins"))
                            {
                                rights.Add("can admin");
                                rights.Add("can edit");
                                rights.Add("can view");
                            }
                            if (membership.Contains("users"))
                            {
                                rights.Add("can view");
                            }
                            string rights_json = Newtonsoft.Json.JsonConvert.SerializeObject(rights);

                            List<Claim> claims = new List<Claim>();
                            claims.Add(new Claim("sub", user));
                            claims.Add(new Claim("scope", scope_json));
                            claims.Add(new Claim("rights", rights_json));

                            string key_s = ConfigurationManager.ConnectionStrings["JWTKey"].ConnectionString;
                            byte[] key_b = System.Text.Encoding.UTF8.GetBytes(key_s);
                            SigningCredentials creds = new SigningCredentials(
                                new InMemorySymmetricSecurityKey(key_b),
                                "http://www.w3.org/2001/04/xmldsig-more#hmac-sha256",
                                "http://www.w3.org/2001/04/xmlenc#sha256");

                            JwtSecurityToken token = new JwtSecurityToken(
                                issuer: "http://testauth.plasne.com",
                                audience: null,
                                claims: claims,
                                expires: DateTime.Now.AddHours(4),
                                signingCredentials: creds
                                );

                            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
                            string jwt = handler.WriteToken(token);

                            response.StatusCode = HttpStatusCode.Redirect;
                            Config c = new Config();
                            response.Headers.Location = new Uri(c.GetValue("homepage") + "?accessToken=" + jwt);

                        }
                        catch (Exception ex)
                        {
                            string message = Regex.Replace(ex.Message, @"\t|\n|\r", "");
                            response.StatusCode = HttpStatusCode.Unauthorized;
                            response.ReasonPhrase = "Unauthorized (details): " + message; // + ex.Message.Replace("\n", " ");
                            response.Headers.Add("exception1", message);
                        }

                    }
                    catch (Exception ex)
                    {
                        response.StatusCode = HttpStatusCode.Unauthorized;
                        response.ReasonPhrase = "Unauthorized (membership): " + ex.Message;
                        response.Headers.Add("exception2", ex.Message);
                    }

                }
                catch (Exception ex)
                {
                    response.StatusCode = HttpStatusCode.Unauthorized;
                    response.ReasonPhrase = "Unauthorized (access token): " + ex.Message;
                }

            }
            else
            {
                response.StatusCode = HttpStatusCode.BadRequest;
                response.ReasonPhrase = "Bad Request: this does not appear to be part of the same authorization chain.";
            }

            return response;
        }

    }





}