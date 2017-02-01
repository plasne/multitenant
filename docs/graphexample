
```javascript
// import
var config = require("config");
var adal = require("adal-node");
var request = require("request");
 
// set global variables
var authority = config.get("authority");
var directory = config.get("directory");
var clientId = config.get("clientId");
var clientSecret = config.get("clientSecret");
 
// authenticate
var context = new adal.AuthenticationContext(authority + directory);
context.acquireTokenWithClientCredentials("https://graph.microsoft.com/", clientId, clientSecret, function(error, tokenResponse) {
    if (!error) {
       
request.get({
            "uri": "https://graph.microsoft.com/v1.0/users",
            "headers": {
                "Authorization": "bearer " + tokenResponse.accessToken
            }
        }, function(error, response, body) {
            if (!error && response.statusCode == 200) {
                console.log(body);
            } else {
                if (error) { console.log("error(101): " + error) } else { console.log("error(102)"); console.log(body); };
            }
        });
               
    } else {
        console.log("error(100): " + error);
    }
});
```

The Application Permissions were set to "Read directory data".
