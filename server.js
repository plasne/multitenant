
// import everything that is required
var config = require("config");
var express = require("express");
var q = require("q");
var request = require("request");
var crypto = require("crypto");
var qs = require("querystring");
var cookieParser = require("cookie-parser");
var nJwt = require("njwt");
var ad = require("activedirectory");
var AuthenticationContext = require("adal-node").AuthenticationContext;
var aadutils = require("./aadutils.js");

// get the configuration
var port = config.get("web.port");
var clientId = config.get("aad.clientId");
var clientSecret = config.get("aad.clientSecret");
var authority = config.get("aad.authority");
var redirectUri = config.get("aad.redirectUri");
var resource = config.get("aad.resource");
var jwtKey = config.get("jwt.key");

// create the web server
var app = express();
app.use(cookieParser());
app.use(express.static("public"));

// redirect to the main page
app.get("/", function(req, res) {
  res.redirect("/client.html");
});

// redirect through the AAD consent pattern
function consent(res, add) {
  crypto.randomBytes(48, function(err, buf) {
    if (err) {
      res.status(500).send("Server Error: a crypto token couldn't be created to secure the session.");
    } else {
      var token = buf.toString("base64").replace(/\//g, "_").replace(/\+/g, "-");
      res.cookie("authstate", token);
      var url = authority + "/oauth2/authorize?response_type=code&client_id=" + qs.escape(clientId) + "&redirect_uri=" + qs.escape(redirectUri) + "&state=" + qs.escape(token) + "&resource=" + qs.escape(resource) + add;
      res.redirect(url);
    }
  });
}

// a login with administrative consent
app.get("/consent", function(req, res) {
  consent(res, "&prompt=admin_consent");
});

// a login with user consent (if the admin has already consented there is no additional consent required)
app.get("/login/aad", function(req, res) {
  consent(res, "");
});

// Azure AD will first return a code that can then be converted into an access token with rights as defined for the app
function getAccessTokenFromCode(code) {
  var deferred = q.defer();

  var authenticationContext = new AuthenticationContext(authority);
  authenticationContext.acquireTokenWithAuthorizationCode(
    code,
    redirectUri,
    resource,
    clientId, 
    clientSecret,
    function(err, response) {
      if (err) {
        deferred.reject(err.message);
      } else {
        deferred.resolve(response);
      }
    }
  );

  return deferred.promise;
}

// query the customer's Azure AD to find out what groups the user is a member of
function getGroupMembershipForUser(token, domain, userId) {
  var deferred = q.defer();

  var options = {
    uri: "https://graph.windows.net/" + domain + "/users/" + userId + "/getMemberGroups?api-version=1.6",
    json: true,
    headers: {
      "Authorization": "bearer " + token
    },
    body: {
      "securityEnabledOnly": false
    }
  };
  request.post(options, function(error, response, body) {
    if (!error && response.statusCode == 200) {
        deferred.resolve(body.value);
    } else {
        deferred.reject(JSON.stringify(body));
    }
  });

  return deferred.promise;
}

// get the details about the groups (notably the display name) from the customer's AD
function getGroupDetails(token, domain, groups) {
  var deferred = q.defer();
  
  var options = {
    uri: "https://graph.windows.net/" + domain + "/getObjectsByObjectIds?api-version=1.6",
    json: true,
    headers: {
      "Authorization": "bearer " + token
    },
    body: {
        "objectIds": groups,
        "types": [ "group" ]
    }
  };
  request.post(options, function(error, response, body) {
    if (!error && response.statusCode == 200) {
        deferred.resolve(body.value);
    } else {
        deferred.reject(JSON.stringify(body));
    }
  });
  
  return deferred.promise;
}

function getJwtFromToken(token, userId) {
    var deferred = q.defer();
    
    // get the membership for the user
    var domain = userId.split("@")[1];
    getGroupMembershipForUser(token, domain, userId).then(function(groups) {
        
        // get the details for each group
        getGroupDetails(token, domain, groups).then(function(details) {
            
            // build a list of group names
            var membership = [];
            details.forEach(function(group) {
                if (group.displayName.startsWith("testauth_")) {
                membership.push(group.displayName.replace("testauth_", ""));
                }
            });

            // define rights
            var rights = [];
            if (membership.indexOf("admins") > -1) {
                rights.push("can admin");
                rights.push("can edit");
                rights.push("can view");
            } else if (membership.indexOf("users") > -1) {
                rights.push("can view");
            }

            // build the claims
            var claims = {
                iss: "http://testauth.plasne.com",
                sub: userId,
                scope: membership,
                rights: rights
            };

            // build the JWT
            var jwt = nJwt.create(claims, jwtKey);
            jwt.setExpiration(new Date().getTime() + (4 * 60 * 60 * 1000)); // 4 hours
            deferred.resolve(jwt.compact());
            
        }, function(msg) {
            deferred.reject(msg);
        });
        
    }, function(msg) {
        deferred.reject(msg);
    });
    
    return deferred.promise;
}

// get an authorization token
app.get('/token', function(req, res) {

  // ensure this is all part of the same authorization chain
  if (req.cookies.authstate !== req.query.state) {
    res.status(400).send("Bad Request: this does not appear to be part of the same authorization chain.");
  } else {
    
    // get the access token
    getAccessTokenFromCode(req.query.code).then(function(tokenResponse) {

      // generate a JWT
      getJwtFromToken(tokenResponse.accessToken, tokenResponse.userId).then(function(jwt) {
          
        // return the JWT to the client
        res.cookie("accessToken", jwt, {
          maxAge: 4 * 60 * 60 * 1000 // 4 hours
        });
        res.redirect("/client.html");

      }, function(msg) {
        res.status(401).send("Unauthorized (jwt): " + msg);
      });
        
    }, function(msg) {
      res.status(401).send("Unauthorized (access token): " + msg);
    });

  }
  
});

// login to a traditional Active Directory
app.get("/login/ad", function(req, res) {
  
  // connect to AD
  var adConfig = config.get("ad");
  var client = new ad(adConfig);
  
  // authenticate the user
  var credentials = JSON.parse(req.cookies.credentials);
  client.authenticate(credentials.username, credentials.password, function(err, auth) {
    if (err) {
        res.status(401).send(JSON.stringify(err));
    }
    if (auth) {
      client.getGroupMembershipForUser(credentials.username, function(err, groups) {
        if (err) {
            res.status(500).send(JSON.stringify(err));
        }
        if (groups) {
            
            // build a list of group names
            var membership = [];
            groups.forEach(function(group) {
                if (group.cn.startsWith("testauth_")) {
                    membership.push(group.cn.replace("testauth_", ""));
                }
            });

            // define rights
            var rights = [];
            if (membership.indexOf("admins") > -1) {
                rights.push("can admin");
                rights.push("can edit");
                rights.push("can view");
            } else if (membership.indexOf("users") > -1) {
                rights.push("can view");
            }
 
            // build the claims
            var claims = {
                iss: "http://testauth.plasne.com",
                sub: credentials.username,
                scope: membership,
                rights: rights
            };

            // build the JWT
            var jwt = nJwt.create(claims, jwtKey);
            jwt.setExpiration(new Date().getTime() + (4 * 60 * 60 * 1000)); // 4 hours
            res.cookie("accessToken", jwt.compact(), {
                maxAge: 4 * 60 * 60 * 1000
            });

            // return to the client
            res.status(200).end();

        }
      });
    } else {
        res.status(401).send("Unknown authorization failure.");
    }
  });
});

function verifyToken(token) {
  var deferred = q.defer();  

  // get the public keys
  var options = {
    uri: "https://login.microsoftonline.com/common/discovery/keys", // the key URL comes from: https://login.microsoftonline.com/<tenantId>/.well-known/openid-configuration
    json: true
  };
  request.get(options, function(error, response, body) {
    if (!error && response.statusCode == 200) {

      // try each public key
      body.keys.forEach(function(key) {
        var modulus = new Buffer(key.n, "base64");
        var exponent = new Buffer(key.e, "base64");
        var pem = aadutils.rsaPublicKeyPem(modulus, exponent);
        nJwt.verify(token, pem, "RS256", function(err, verified) {
            if (err) {
                deferred.reject("Unauthorized (verify token): " + err);
            } else {
                if (verified.body.aud == "http://testauth.plasne.com/") {
                    deferred.resolve(verified);
                } else {
                    deferred.reject("Unauthorized (aud): The token was generated for the wrong audience.");
                }
            }
        });
      });

    } else {
        deferred.reject("Unauthorized (get keys): " + error);
    }
  });
  
  return deferred.promise;
}

// the user has logged in to Azure AD and obtained a token already, in that case, validate the token and generate the JWT
app.get("/login/token", function(req, res) {
    
    // verify the existing token
    var token = req.get("Authorization").replace("Bearer ", "");
    verifyToken(token).then(function(verified) {
        
        // native apps cannot do admin consent, so we cannot reach back into the user's AAD, but we can generate a JWT without any special
        //   authorization and assume we do that in our application
        var claims = {
            iss: "http://testauth.plasne.com",
            sub: verified.body.upn
        };

        // build the JWT
        var jwt = nJwt.create(claims, jwtKey);
        jwt.setExpiration(new Date().getTime() + (4 * 60 * 60 * 1000)); // 4 hours
        res.status(200).send({ "accessToken": jwt.compact() });

    }, function(msg) {
        res.status(401).send(msg);
    });
 
});

// this is a service end-point that can verify the JWT coming from the client
app.get("/whoami", function(req, res) {
  var token;
  if (req.cookies.accessToken) token = req.cookies.accessToken;
  if (req.get("Authorization")) token = req.get("Authorization").replace("Bearer ", "");
  if (token) {
    nJwt.verify(token, jwtKey, function(err, verified) {
      if (err) {
        console.log(err);
      } else {
        var role = "none";
        if (verified.body.scope) {
          if (verified.body.scope.indexOf("users") > -1) role = "user";
          if (verified.body.scope.indexOf("admins") > -1) role = "admin";
        }
        res.send({
          "id": verified.body.sub,
          "role": role,
          "rights": (verified.body.rights) ? verified.body.rights : "none"
        });
      }
    });
  } else {
    res.status(401).send("Not Authorized: no access token was passed");
  }
});

app.listen(port);
console.log("listening on " + port);
