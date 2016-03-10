# JavaScript client
Perhaps the most common way to connect to a collection of REST/JSON services is via the browser, so this sample contains an HTML/JS sample.

The files are hosted by the [Node.js server](nodejs.md) from the public/ folder. There is simply a client.html and client.js file, but you will notice there are some libraries included via CDN:

* [jQuery](https://jquery.com/)
* [jQuery Cookie](https://github.com/carhartl/jquery-cookie)

## Consent
The consumers will each have their own Azure Active Directory with their own users and their own groups. The provider does not have access to these directories until a Global Administrator of a consumer directory consents to granting the application some specific rights. The rights they need to grant were chosen when the application was [created](ad-application.md).

The sample takes the administrator through the consent process when the "consent" link is clicked on the client.html page. You will see in the client.js that this simply directs the browser to the /consent endpoint hosted in the Node.js server. The complete process is:

1. The administrator logs in to their own Azure Active Directory.
2. The administrator agrees to give the application certain rights.
3. The rest of the login process described below in Login-AAD starting with step 2 is performed.

## Login-AAD
There is also a "login-aad" link on the client.html page that directs the browser to the /login/aad. This takes a user through the login process. This must be done after the administrator has consented. The complete process is:

1. The user logs in to their own Azure Active Directory.
2. The user is given an access code (AuthN).
3. The server takes the access code and turns it into an access token (AuthZ for whatever rights were granted the Azure AD application).
4. The server uses the access code to query the consumer's Azure Active Directory asking for the groups the user is a member of.
5. A JWT is generated with claims for the user's identity, group membership, rights, etc. It will be returned to the user via a cookie.
6. The user is returned to the client.html page and will see an "authenticated" message.

## Login-AD
If you provide a valid username and password, you can then click on the "login-ad" link on the client.html page. Unlike the above examples which use Azure AD, this process authenticates a user against an Active Directory Domain Services server. This process does not require the consent workflow. The complete process is:

1. The supplied credentials are authenticated via LDAP.
2. The LDAP directory is queried for groups that the user is a member of using the provided service account.
3. A JWT is generated with claims for the user's identity, group membership, rights, etc. It will be returned to the user via a cookie.
4. The user is shown the "authenticated" message. Unlike the prior flows which involved browser redirects, this method can stay on the page.

## Who-Am-I
The who-am-I links on the page allow you to connect to a number of backend services (one hosted in Node.js, one hosted as a Web API service, and one hosted as a WCF service) which will validate the JWT and provide details about the user. This is a service to demonstrate how the AuthN and AuthZ will be used to contact web services. The complete process is:

1. The client sends the JWT to the service either as a cookie or as an Authorization header.
2. The server uses the symmetric key to decrypt the JWT signature, thereby validating it's authenticity.
3. The server provides information about the user's claims (from the token) back to the client.
