# Java authentication server

This sample inclues a Java server that can authenticate a user against Azure Active Directory and then provide the client a JSON Web Token that can be passed to a service as proof of identity and claims about the user's access rights.

I am not a Java programmer, and so had to learn a number of things for this project.

* Ant - I used this for the build process.
* Ivy - This is a package manager that works with Ant.

## Components

You can find this solution in the java/ folder using the following file structure:

* build/ - the properties for Ant
* deploy/ - the WAR file that will be deployed to Tomcat
* jsp/ - the JSP, JavaScript, HTML, CSS, etc. files
* lib/ - the JAR files that are the dependencies for this project; this is populated by Ivy
* res/ - the resources required for the project, in this case, just the configuration parameters
* src/ - the JAVA source files
* web/ - the web.xml file that declares the servlets
* build.xml - the build specification for the project
* ivy.xml - the listing of project dependencies

The dependencies for this project included:

* com.microsoft.azure, adal4j - the Microsoft ADAL library (used for authentication)
* com.nimbusds, oauth2-oidc-sdk - an OAuth2 library which includes JWT functionality
* org.json, json - a library to serialize and deserialize JSON

These are many dependencies that are installed when the above components are resolved. You can find a full list in the lib/ folder.

There are some files that are specifically related to making this work:

* src/Login.java - This servlet handles the web requests to handle the consent and login processes.
 
* src/Token.java - This servlet is called after an "access code" has been obtained. It will get an "access token", determine the access rights, and build a JWT to return to the client.
 
* res/multitenant.sample.properties - This contains the configuration for the solution.

* res/index.jsp, res/index.js - These are web files in case you want to host the client using Java as well.

## CORS

TODO TODO TODO

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
