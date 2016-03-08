using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.Security;
using System.Web.SessionState;

namespace TestAuthNET
{
    public class Global : System.Web.HttpApplication
    {

        public static void RegisterWebApi(HttpConfiguration config)
        {

            // remove the Controller suffix requirement for Web API
            var suffix = typeof(DefaultHttpControllerSelector).GetField("ControllerSuffix", BindingFlags.Static | BindingFlags.Public);
            if (suffix != null) suffix.SetValue(null, string.Empty);

            // routes
            config.MapHttpAttributeRoutes();

        }

        protected void Application_Start(object sender, EventArgs e)
        {
            GlobalConfiguration.Configure(RegisterWebApi);
        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected string ValidReferrerDomain(string urlRef)
        {
            if (!string.IsNullOrEmpty(urlRef))
            {
                Uri uriRef = new Uri(urlRef);

                // get the protocol for the referenced url
                string uriProtocol = uriRef.Scheme;

                if (uriProtocol == "http" || uriProtocol == "https")
                {
                    // we have a valid scheme - verify the referrer domain is valid
                    string uriHost = uriRef.Host;

                    if (uriHost.EndsWith(".plasne.com", StringComparison.OrdinalIgnoreCase) ||
                        (uriHost.StartsWith("pelasne-", StringComparison.OrdinalIgnoreCase) &&
                            uriHost.EndsWith(".azurewebsites.net", StringComparison.OrdinalIgnoreCase)))
                    {
                        return uriProtocol + "://" + uriHost;
                    }
                }
            }

            // invalid domain specified
            return string.Empty;
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

            // extract the referrer from the request
            HttpApplication app = (HttpApplication)sender;
            string referrer = app.Request.Headers["Referer"];
            if (string.IsNullOrEmpty(referrer))
            {
                referrer = app.Request.Headers["Origin"];
            }

            // if the request is coming from a valid referer domain, formulate a CORS response
            string domain = ValidReferrerDomain(referrer);
            if (!string.IsNullOrEmpty(domain))
            {
                app.Response.Headers.Add("Access-Control-Allow-Origin", domain);
                if (app.Request.HttpMethod.Equals("OPTIONS"))
                {
                    Response.Flush();
                    app.CompleteRequest();
                }
            }

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}