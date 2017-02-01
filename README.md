**This sample should be updated to:**
 - Use https://graph.microsoft.com instead of https://graph.windows.net (including projection). [example](/docs/graphexample.md)




# Multi-Tenant AuthN/AuthZ Sample
This idea behind this multi-tenant application is that one company (who I will refer to as the **provider**) owns a set of web services that they host in their domain and have registered in their Azure Active Directory. They have multiple technologies that they use to host these services (Node.js, .NET WCF, .NET Web API, Java, etc.).

There are multiple other companies (who I will refer to as **consumers**) that will consume those services using a variety of clients (HTML/JS, mobile apps, WPF apps, etc.). These consumers will manage their own users and groups via their own Active Directory systems.

There are a variety of pieces provided:

- Authentication Services
  - A Node.js server presenting a simple HTML/JS site and REST/JSON services
    - AuthN/AuthZ can be via Azure AD or Active Directory
    - A JSON Web Token is always provided regardless of the authentication method
    - The JSON Web Token contains any claims about the user's identity or rights
    - The services 
  - A Web API server providing the same functionality as the Node.js server
  - A Java server providing the same functionality as the Node.js server
- Service Providers
  - A Node.js service that will respond after it can verify the JWT
  - A WCF service that will respond after it can verify the JWT
  - A Web API service that will respond after it can verify the JWT
  - An Azure Function service that will respond after it can verify the JWT
- Clients
  - A JavaScript client that can consume those services after authenticating
  - A WPF client that can consume those services after authenticating

## Samples
Click on any of the following to understand the specifics of the configuration:

- Authentication Services
  - [Node.js server](/docs/nodejs.md)
  - [Web API server](/docs/webapi-auth.md)
  - Java server
- Service Providers
  - Node.js service
  - WCF service
  - [WebAPI client](/docs/webapi.md)
  - [Azure Function service](https://github.com/plasne/multitenant-func)
- Clients
  - [JavaScript client](/docs/javascript.md)
  - [WPF client](/docs/wpf.md)



## To-Do
- As soon as it is supported, drop "Read all groups" or "Read directory data", and use "Sign in and read user profile" and "Access the directory as the signed-in user".

||Single-Tenant|Multi-tenant|Comments|
|---|---|---|---|
|Native||||
|---|---|---|---|
|  Username/Password|Works|Unsupported|As expected|
|  ClientID/ClientSecret|Insufficient privileges|Unsupported|As expected; there doesn't seem to be a way to properly establish permissions so it seems legit that this would not work.|
|Web App||||
|---|---|---|---|
|  Username/Password|Request body must contain client_secret or client_assertion|Request body must contain client_secret or client_assertion|This is consistent, but seems like a scenario that should be OK; I suspect this might get fixed in a later release.|
|  ClientID/ClientSecret|Works|The identity of the calling app cannot be established|This is inconsistent between single- and multi- tenant so must be a bug.|
