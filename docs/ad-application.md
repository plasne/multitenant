#Create an Azure AD Application
The primary method of authentication for this sample is Azure AD. Specifically, this assumes you will create an application that is multi-tenant, allowing your customers to authenticate using their own Azure AD and managing their own users and group membership.

All of these steps are done in the Azure AD Directory of the company that owns the multi-tenant application, the customers of your application do not go through these steps.

##Verify your domain
You can only host a multi-tenant application on a domain that you own and have verified. To verify you own the domain you must add it to the Azure AD Directory that will own your application. If you have already verified your domain (often the case if you are an Office 365 customer), then you don't need to do these steps.

To verify the domain, follow these steps:

1. Login to https://manage.windowsazure.com.
2. Click on "Active Directory" in the left-hand navigation pane.
3. Click on the Directory that will own the application.
4. Click on the "Domains" tab at the top.
5. Click on "Add" at the bottom to add a new domain.
6. Type you domain name. You don't need to flag the single sign-on option unless you are also using this domain for your company logins (outside the scope of this article).
7. Click "add".
8. Click the arrow at the bottom to advance to the verify page.
9. Make the changes as described to your DNS and press "verify".

##Create an Azure Active Directory Application

Once you have a verified domain you can add applications to it.

To create a new Azure AD application follow these steps:

1. Login to https://manage.windowsazure.com.
2. Click on "Active Directory" in the left-hand navigation pane.
3. Click on the Directory that will own the application.
4. Click on the "Applications" tab at the top.
5. Click to "Add" an application at the bottom.
6. Click on "Add an application my organization is developing".
7. Provide a name for your application.
8. Choose "Web application and/or web API".
9. Provide a "Sign-On URL" and an "App ID URI". These can be the same thing and typically should be the URL for your web APIs. For example, for my application I chose "https://testauth.plasne.com/". **IMPORTANT:** Your Sign-On URL must end in a trailing slash and it must be a domain that you own and have verified.
10. Click the checkmark to create the application.
11. Click on the "Configure" tab.
12. Click "yes" under Application is multi-tenant".
13. Make note of the "Client ID", you will use that when you configure your server and clients.
14. "Select duration" to create a key (also called a Client Secret).
15. Under "permissions to other applications", "Windows Azure Active Directory", "Delegated Permissions", check "Sign in and read user profile" (it is probably selected already).
16. Click on the "Save" button at the bottom.
17. Copy the key that is generated after save under "keys". This is the Client Secret and you will need it for your server implementation. **IMPORTANT:** You cannot get this after your leave the page, so you must copy it now or create a new key later.

[//]: # (Read directory data is actually required?)
[//]: # (Read all groups is actually required?)
