using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
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

        private string ConsentUri(string add = "")
        {
            Config c = new Config();
            string key = GetKey();
            string url = c.GetValue("authority") + "/oauth2/authorize?response_type=code&client_id=" + c.GetEncodedValue("clientId") + "&redirect_uri=" + c.GetEncodedValue("redirectUri") + "&state=" + c.GetEncodedValue(key) + "&resource=" + c.GetEncodedValue("resource") + add;
            return url;
        }

        [HttpGet, Route("consent")]
        public void Consent()
        {
            Redirect(ConsentUri("&prompt=admin_consent"));
        }

        [HttpGet, Route("login")]
        public void Login()
        {
            Redirect(ConsentUri());
        }

        [HttpGet, Route("token")]
        public string Token()
        {
            return "got this far";
        }

    }
}