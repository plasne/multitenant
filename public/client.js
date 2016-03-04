
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
    window.location = "/login/ad";
  });

  $("#whoami").click(function() {
    $.ajax({
      url: "/whoami",
      success: function(me) {
        $("<div />").appendTo("body").text("id: " + me.id + " (" + me.role + ")");
      },
      error: function(err) {
        $("<div />").appendTo("body").text("error: " + err);
      }
    });
  });
    
});
