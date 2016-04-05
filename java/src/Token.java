import java.io.*;
import java.util.concurrent.*;
import java.net.*;
import javax.servlet.*;
import javax.servlet.http.*;

import com.microsoft.aad.adal4j.*;
import com.nimbusds.oauth2.sdk.AuthorizationCode;
import com.nimbusds.openid.connect.sdk.AuthenticationErrorResponse;
import com.nimbusds.openid.connect.sdk.AuthenticationResponse;
import com.nimbusds.openid.connect.sdk.AuthenticationResponseParser;
import com.nimbusds.openid.connect.sdk.AuthenticationSuccessResponse;

public class Token extends HttpServlet {

  public void init() throws ServletException
  {
  }

  public String getCookie(String name, HttpServletRequest request) {
    Cookie[] cookies = request.getCookies();
    if (cookies != null) {
      for (int i = 0; i < cookies.length; i++) {
        if (cookies[i].getName().equals(name)) {
          return cookies[i].getValue();
        }
      }
    }
    return null;
  }

  public void doGet(HttpServletRequest request, HttpServletResponse response) throws ServletException, IOException
  {

      // move to configuration file
      String authority = "https://login.windows.net/common";
      String clientId = "36bda7c5-cc23-4618-9e09-e710b2357818";
      String clientSecret = "gol9M4EXyLk0lNJWzig2FUECrMJnh7TvEdm+KLHo7rk=";
      String redirect = "http://pelasne-java.southcentralus.cloudapp.azure.com:8080/testauth/token";
      String resource = "https://graph.windows.net/";

      // ensure this is part of the same authorization chain
      String state_qs = request.getParameter("state");
      String state_c = getCookie("authstate", request);
      if (!state_qs.equals(state_c)) {
        response.sendError(400, "Bad Request: this does not appear to be part of the same authorization chain (" + state_qs + ", " + state_c + ").");
      } else {

        ExecutorService service = null;
        try {

          service = Executors.newFixedThreadPool(1);
          AuthenticationContext authContext = new AuthenticationContext(authority, true, service);

          Future<AuthenticationResult> promise = authContext.acquireTokenByAuthorizationCode(
            request.getParameter("code"),
            new URI(redirect),
            new ClientCredential(clientId, clientSecret),
            resource,
            null);

          AuthenticationResult result = promise.get();
          if (result == null) {
            response.sendError(401, "no auth result");
          } else {
            response.sendError(500, "success " + result.getAccessToken());
          }

        } catch (Throwable e) {
          response.sendError(500, "exception - " + e.getCause() + "; " + e.getMessage() + "; " + e.getStackTrace());
        } finally {
          service.shutdown();
        }

      }


  }

  public void destroy()
  {
      // do nothing.
  }
}
