# Web API authentication server

This sample inclues a Web API server that can authenticate a user against Azure Active Directory and then provide the client a JSON Web Token that can be passed to a service as proof of identity and claims about the user's access rights.

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
 
* Web.config - There are custom headers required for CORS to work. In addition, there are some secrets (such as the JWT symmetric key) that can be stored here. If this solution is going to be hosted in Azure, you can put those secrets in the Azure App Settings and/or Connection Strings.

* WebAPIService.cs - The file that contains the authentication endpoints.

## CORS

There are 2 pieces to getting CORS to work. The first is a set of custom response headers in the web.config file; specifically under configuration/system.webServer/httpProtocol/customHeaders:

* Access-Control-Allow-Credentials
* Access-Control-Allow-Methods
* Access-Control-Allow-Headers   (make sure Authorization is included)
* Access-Control-Max-Age

The second piece is to make Access-Control-Allow-Origin respond with the referrer domain provided it was a legitimate domain. This is implemented in the Global.asax file in the method ValidReferrerDomain. Essentially if the call was made from a domain that you want to respond to, you write back the host URL as the origin.

## Create an Azure AD Application

The first step is to create an Azure AD Application. You can find those instructions [here](ad-application.md).

## Store the configuration settings

There are a series of configuration settings that need to be stored somewhere. These could be stored in the web.config file, but if you are using Azure, it is better to store those as App Settings in Azure. This sample assumes everything is stored in Azure App Settings.

* authority: This should always be "https://login.windows.net/common".
* clientId: This is the Client ID of your application as described [here](ad-application.md).
* clientSecret: This is the Client Secret of your application as described [here](ad-application.md).
* redirectUri: This must be the Redirect URI of your application as described [here](ad-application.md).
* resource: This should be "https://graph.windows.net/".
* homepage: This should be the home page of your application. Where it will redirect to when all authentication processes are complete.
* JWTKey: You should generate a long, random string of characters to use as the symmetric key for all your backend services. They will need this key to decrypt the JWT, you should never store this in an unsecure location or send it to a client.

## Security

**This sample shows the server running on HTTP, but you should always host a service like this for authentication using HTTPS.**

## Configure DNS

In order for the authentication flow to work your server must be hosted on the verified domain that you specified when creating the application. Therefore you must register the FQDN for your application with your DNS provider so that it can route properly to your server.

## Run the server

You can run the server on any system that can operate as a .NET web server. This could also be hosted in Azure as a Web App.

## Process flow

There is a consent process that must be done once before the standard login process can be used.

# Consent

1. An administrator clicks a "consent" link or similar in a browser.
2. The browser is redirected to the /consent endpoint (WebAPIAuth.cs).
3. The /consent endpoint generates a random "state" key that is just used to track that all these redirects are part of the same authentication flow.
4. The browser is redirected to Azure AD OAuth authorize endpoint.
5. The administrator is asked to consent to the rights the application needs for all users in his organization (for example, this app needs to read the objects in the directory).
6. The browser is redirected to the STS provider for the consumer (this could be Azure AD, but could also be something like ADFS or OKTA).
7. The administrator authenticates however he is allowed (for example, username/password, multi-factor authentication, etc.).

The rest of the steps are the same starting with step #8 in the Login process below.

# Login

1. A user clicks a "login" link or similar in a browser.
2. The browser is redirected to the /login endpoint (WebAPIAuth.cs).
3. The /login endpoint generates a random "state" key that is just used to track that all these redirects are part of the same authentication flow.
4. The browser is redirected to Azure AD OAuth authorize endpoint.
5. The access rights that are requested by the app are checked to ensure they have been granted. If everything was done properly with the consent, this process is not seen by the user.
6. The browser is redirected to the STS provider for the consumer (this could be Azure AD, but could also be something like ADFS or OKTA).
7. The user authenticates however he is allowed (for example, username/password, multi-factor authentication, etc.).
8. The STS provider generates an "access code" to pass back in the querystring.
9. The browser is redirected to the /token endpoint (WebAPIAuth.cs).
10. The /token endpoint performs the following steps:
  1. The "state" key is checked to make sure this is all part of the same authentication chain.
  2. The "access code" is sent to Azure AD expecting an "access token" in response.
  3. The "access token" is used to query the Azure AD Graph API to get the list of groups the user is a member of. This is returned as a list of IDs.
  4. The "access token" is used to query the Azure AD Graph API to get the name of the groups from the IDs.
  5. A JSON Web Token is generated that contains information about the user and what rights he has access to in your app (based on the group membership). The JWT is signed by the symmetric key.
11. The browser is redirected to the home page for your application. The JWT is a parameter in the querystring.
  
## Authentication vs Authorization

You will notice in the authentication workflow that both an "access code" and "access token" are generated.

The "access code" is generated by the consumer's STS provider - this is authentication. This code provides no claims about the rights that the user or application might have, it is simply a way to validate that the user has authenticated successfully.

The "access token" is generated by Azure AD - this is authorization. This token is a JWT that contains claims about the user's identity and provides authority for the user or application to exercise whatever rights it has been granted.
