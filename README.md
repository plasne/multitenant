# Mulit-Tenant AuthN/AuthZ
This sample code is provided as an example of hosting a web service that can be consumed by multiple endpoint types using authentication via Active Directory and authorization via group membership. This is a multi-tenant example whereby customers will use their own AD for managing identities and group membership rather than using the AD of the hosting entity.

There are a variety of pieces provided:
- A Node.js server presenting a simple HTML/JS site and REST/JSON services
  - AuthN/AuthZ can be via Azure AD, LDAP, or client certificate (Coming Soon)
  - A JSON Web Token is always provided regardless of 
- A JavaScript client that can consume those services after authenticating
- A WCF client that can consume those services after authenticating
- A WebAPI client that can consume those services after authenticating
- A WPF client that can consume those services after authenticating
- (Coming Soon) A WPF client that is on a trusted endpoint that can consume those services after authenticating
- (Coming Soon) A Java server providing the same functionality as the Node.js server
