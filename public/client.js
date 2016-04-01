
function getQuerystring(key, default_)
 {
   if (default_==null) default_=""; 
   key = key.replace(/[\[]/,"\\\[").replace(/[\]]/,"\\\]");
   var regex = new RegExp("[\\?&]"+key+"=([^&#]*)");
   var qs = regex.exec(window.location.href);
   if(qs == null)
     return default_;
   else
     return qs[1];
 }

$(document).ready(function() {

  var accessToken_qs = getQuerystring("accessToken");
  if (accessToken_qs.length > 0) {
      $.cookie("accessToken", accessToken_qs);
  }

  if (document.cookie.indexOf("accessToken") > -1) {
    $("<div />").appendTo("#status").text("authenticated");
  }

  $("#node-login-ad").click(function() {
    var credentials = {
        username: $("#node-login-ad-username").val(),
        password: $("#node-login-ad-password").val()
    };
    $.cookie("credentials", JSON.stringify(credentials));
    $.ajax({
       url: "/login/ad",
       success: function() {
           $("<div />").appendTo("#status").text("authenticated");
       },
       error: function(err) {
           $("<div />").appendTo("#status").text("error: " + err);
       }
    });
  });

  $("#node-whoami").click(function() {
    $.ajax({
      url: "/whoami",
      success: function(me) {
        $("<div />").appendTo("#status").text("id: " + me.id + " (" + me.role + ") " + me.rights);
      },
      error: function(err) {
        $("<div />").appendTo("#status").text("error: " + err);
      }
    });
  });
  
  $("#webapi-whoami").click(function() {
    $.ajax({
      url: "https://pelasne-testauth.azurewebsites.net/hello",
      headers: {
        "Authorization": "Bearer " + $.cookie("accessToken")
      },
      dataType: "json",
      success: function(reply) {
        $("<div />").appendTo("#status").text(reply.message);
      },
      error: function(err) {
        $("<div />").appendTo("#status").text("error: " + err);
      }
    });
  });

  $("#wcf-whoami").click(function() {
    $.ajax({
      url: "http://pelasne-testauth.azurewebsites.net/wcfservice.svc/hello",
      headers: {
        "Authorization": "Bearer " + $.cookie("accessToken")
      },
      dataType: "json",
      success: function(reply) {
        $("<div />").appendTo("#status").text(reply.message);
      },
      error: function(err) {
        $("<div />").appendTo("#status").text("error: " + err);
      }
    });
  });
  
  $("#func-whoami").click(function() {
      $.ajax({
          url: "https://pelasne-func.azurewebsites.net/api/testauth?code=wxoeguz9j0y2k6oh3wqoxbt9t1e3w6iibdv3p2ogveso47vih9n4dioc7w4pclldfiara4i",
          headers: {
              "Authorization": "Bearer " + $.cookie("accessToken")
          },
          dataType: "json",
          success: function(me) {
              $("<div />").appendTo("#status").text("id: " + me.id + " (" + me.role + ") " + me.rights);
          },
          error: function(err) {
              $("<div />").appendTo("#status").text("error: " + err);
          }
      });
  });
  
});
