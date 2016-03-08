
$(document).ready(function() {

  if (document.cookie.indexOf("accessToken") > -1) {
    $("<div />").appendTo("body").text("authenticated");
  }

  $("#consent").click(function() {
    window.location = "/consent";
  });

  $("#login-aad").click(function() {
    window.location = "/login/aad";
  });

  $("#login-ad").click(function() {
    var credentials = {
        username: $("#login-ad-username").val(),
        password: $("#login-ad-password").val()
    };
    $.cookie("credentials", JSON.stringify(credentials));
    $.ajax({
       url: "/login/ad",
       success: function() {
           $("<div />").appendTo("body").text("authenticated");
       },
       error: function(err) {
           $("<div />").appendTo("body").text("error: " + err);
       }
    });
  });

  $("#whoami").click(function() {
    $.ajax({
      url: "/whoami",
      success: function(me) {
        $("<div />").appendTo("body").text("id: " + me.id + " (" + me.role + ") " + me.rights);
      },
      error: function(err) {
        $("<div />").appendTo("body").text("error: " + err);
      }
    });
  });
  
  $("#webapi").click(function() {
    $.ajax({
      url: "https://pelasne-testauth.azurewebsites.net/hello",
      headers: {
        "Authorization": "Bearer " + $.cookie("accessToken")
      },
      dataType: "json",
      success: function(reply) {
        $("<div />").appendTo("body").text(reply.message);
      },
      error: function(err) {
        $("<div />").appendTo("body").text("error: " + err);
      }
    });
  });

  $("#wcfapi").click(function() {
    $.ajax({
      url: "http://pelasne-testauth.azurewebsites.net/wcfservice.svc/hello",
      headers: {
        "Authorization": "Bearer " + $.cookie("accessToken")
      },
      dataType: "json",
      success: function(reply) {
        $("<div />").appendTo("body").text(reply.message);
      },
      error: function(err) {
        $("<div />").appendTo("body").text("error: " + err);
      }
    });
  });
  
});
