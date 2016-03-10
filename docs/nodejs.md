#Node.js server
This sample includes a Node.js server that hosts:
- The REST/JSON services required for AuthN, AuthZ, and sample end-points
- The JavaScript client sample

##Azure Active Directory Application
The primary method of authentication for this sample is Azure AD. Specifically, this assumes you will create an application that is multi-tenant, allowing your customers to authenticate using their own Azure AD and managing their own users and group membership.

To create a new Azure AD application follow these steps:
1- Login to https://manage.windowsazure.com
2- Click on "Active Directory" in the left-hand navigation pane
3- Click on the Directory that will own the application
4- Click on the "Applications" tab at the top
5- Click to "Add" an application at the bottom
6- Click on "Add an application my organization is developing"
7- Provide a name for your application
8- Choose "Web application and/or web API"
9- Provide a "Sign-On URL" and an "App ID URI". These can be the same thing and typically should be the URL for your web APIs. For example, for my application I chose "https://testauth.plasne.com/". **IMPORTANT:** Your Sign-On URL must end in a trailing slash.
10- 
