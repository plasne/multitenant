# Node.js server
This sample includes a Node.js server that hosts:
- The REST/JSON services required for AuthN, AuthZ, and sample end-points
- The JavaScript client sample

## Components
The following are the components that comprise this solution:

* server.js - This is the main entry point for the Node.js server
* public/ - This directory contains the static files that are served by this web server (the JavaScript client sample)
* config/ - The directory containing the configuration files for this solution

There were a number of Node.js components used in this solution:

* [config](https://www.npmjs.com/package/config) - configuration
* [express](https://www.npmjs.com/package/express) - web server
* [q](https://www.npmjs.com/package/q) - promises
* [request](https://www.npmjs.com/package/request) - HTTP request client
* [crypto](https://www.npmjs.com/package/crypto) - key generation
* [qs](https://www.npmjs.com/package/qs) - querystring parsing
* [cookie-parser](https://www.npmjs.com/package/cookie-parser) - cookie management
* [njwt](https://www.npmjs.com/package/njwt) - JSON Web Tokens
* [ad](https://www.npmjs.com/package/activedirectory) - AD interface
* [adal-node](https://www.npmjs.com/package/adal-node) - Azure AD Auth

Also, for using the WPF sample which must verify an existing token via a public key, then aadutils.js is used to create PEM.

## Create an Azure AD Application
The first step is to create an Azure AD Application. You can find those instructions [here](ad-application.md).

## Build a configuration file
You need to define a configuration file for the server. You can find a sample at config/default.sample.json. Simply rename this file to default.json and then fill in the proper information:

- web
  - port: The port that the application will be hosted on.
- aad
  - authority: Leave this as is.
  - clientId: This is the Client ID of your application as described [here](ad-application.md).
  - clientSecret: This is the Client Secret of your application as described [here](ad-application.md).
  - redirectUri: This must be the Redirect URI of your application as described [here](ad-application.md).
  - resource: Leave this as is.
- ad
  - url: This is the IP address of your Active Directory server (or DNS name), followed by the LDAP query port.
  - baseDN: This is the root DN of your Active Directory directory.
  - username: This should be a service account that can query for users/groups.
  - password: This should be the password for the service account.
- jwt
  - key: You should generate a long, random string of characters to use as the symmetric key for all your backend services. They will need this key to decrypt the JWT, you should never store this in an unsecure location or send it to a client.

## Security
**This sample shows the server running on HTTP, but you should always host a service like this for authentication using HTTPS.**

## Configure DNS
In order for the authentication flow to work your server must be hosted on the verified domain that you specified when creating the application. Therefore you must register the FQDN for your application with your DNS provider so that it can route properly to your server.

## Run the server
You can run the server on any system that can run Node.js.

- Ubuntu
  - To install Node.js: https://nodejs.org/en/download/package-manager/ 
  - To run the server: nodejs server.js
  - If you are using a port that requires elevation, you might do: sudo nodejs server.js
- Windows
   - To install Node.js: https://nodejs.org/en/download/ 
   - To run the server: node server.js
   - Make sure the Windows Firewall will allow incoming traffic on that port

If you are hosting the server in Azure, make sure you check your Network Security Groups (on the subnet that your VM is hosted in and on the NIC assigned to your VM) are configured to allow the incoming port.

## Connect via a brower
To connect via a browser, you can go to the URL you specified for your application. To get more information about the JavaScript client, look [here](javascript.md).
