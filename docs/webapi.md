# Web API client
If the provider wants to host some services using Web API (maybe even hosted as an Azure Web App or in Service Fabric), this sample demonstrates how the endpoints that require AuthN/AuthZ can use the JWT provided by the login service to ensure the call is legitimate.

## Components
You can find this Visual Studio 2015 project in the dotnet/ folder (TestAuthNET.sln). Then look at the TestAuthNET project and specifically the WebAPIService.cs file. There are a number of NuGet packages that need to be included as part of this project:

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
