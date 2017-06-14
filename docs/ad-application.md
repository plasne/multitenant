#Create an Azure AD Application
The primary method of authentication for this sample is Azure AD. Specifically, this assumes you will create an application that is multi-tenant, allowing your customers to authenticate using their own Azure AD and managing their own users and group membership.

All of these steps are done in the Azure AD Directory of the provider, the consumers do not go through these steps.

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

1. Login to https://portal.azure.com.
2. Click on "Active Directory" in the left-hand navigation pane.
3. Click on the "App registrations" tab.
4. Click on "New application registration".
5. Choose "Web app / API" for Application Type.
6. Provide a name for your application.
7. Provide a "Sign-On URL". These can be the same thing and typically should be the URL for your web APIs. For example, for my application I chose "https://testauth.plasne.com/". **IMPORTANT:** Your Sign-On URL must end in a trailing slash (I don't think this is actually a requirement any longer) and it must be a domain that you own and have verified.
8. Click on the registered app.
9. Click on "Settings".
10. Click on "Required permissions".
11. You can delete all the existing permissions. You can then add the following new permissions:
    * Microsoft Graph - Delegated Permissions - Sign in and read user profile
    * Microsoft Graph - Delegated Permissions - Access directory as the signed in user
12. Click on "Properties".
13. Click "yes" under Multi-Tenant.
14. Click on "Keys".
15. Create a new key. Make sure you copy this before you leave this pane.
