# Web API services
If the provider wants to host some services using Web API (maybe even hosted as an Azure Web App or in Service Fabric), this sample demonstrates how the endpoints that require AuthN/AuthZ can use the JWT provided by the login service to ensure the call is legitimate.

## Components
You can find this Visual Studio 2015 solution in the dotnet/ folder as TestAuthNET.sln. The project is called TestAuthNET. There are a number of NuGet packages that need to be included as part of this project:

* Microsoft.CodeDom.Providers.DotNetCompilerPlatform
* Microsoft.AspNet.WebApi
* Microsoft.Owin.Security.Jwt
* Microsoft.Owin.Host.SystemWeb

These are dependencies that will get installed when the above components are installed:

* Owin
* Microsoft.Owin
* Microsoft.Owin.Security
* Microsoft.Owin.Security.OAuth
* Newtonsoft.Json
* Microsoft.Net.Compilers
* System.IdentityModel.Tokens.Jwt
* Microsoft.AspNet.WebApi.Client
* Microsoft.AspNet.WebApi.WebHost
* Microsoft.AspNet.WebApi.Core

There are some files that are specifically related to making this work:

* Global.asax - There are 2 things that are done in the global.asax: (1) I removed the Controller suffix requirement for the Web API controllers (this is a personal preference, not a requirement), and (2) I enabled the solution to work with CORS.
 
* Startup.cs - This is responsible for the Web API startup processes, specifically it attaches a JWT authentication component.
 
* Web.config - There are custom headers required for CORS to work.

* WebAPIService.cs - The files that contains any endpoints.

## CORS
There are 2 pieces to getting CORS to work. The first is a set of custom response headers in the web.config file; specifically under configuration/system.webServer/httpProtocol/customHeaders:

* Access-Control-Allow-Credentials
* Access-Control-Allow-Methods
* Access-Control-Allow-Headers   (make sure Authorization is included)
* Access-Control-Max-Age

The second piece is to make Access-Control-Allow-Origin respond with the referrer domain provided it was a legitimate domain. This is implemented in the Global.asax file in the method ValidReferrerDomain. Essentially if the call was made from a domain that you want to respond to, you write back the host URL as the origin.

## Services
The services could be hosted as any class that inherits ApiController. For this example, the only service is called Hello and is contained in the WebAPIService.cs file.

To specify that the service endpoint should require authentication, it is decorated with the [Authorize] attribute
