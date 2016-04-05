// Import required java libraries
import java.io.*;
import java.net.*;
import javax.servlet.*;
import javax.servlet.http.*;

// Extend HttpServlet class
public class Login extends HttpServlet {

  public void init() throws ServletException
  {
  }

  public void doGet(HttpServletRequest request, HttpServletResponse response) throws ServletException, IOException
  {

    // variables
    String authority = "https://login.windows.net/common";
    String clientId = "36bda7c5-cc23-4618-9e09-e710b2357818";
    String redirect = "http://pelasne-java.southcentralus.cloudapp.azure.com:8080/testauth/token";
    String state = "random";
    String resource = "https://graph.windows.net/";

    // write auth cookie
    response.addCookie(new Cookie("authstate", state));

    // consent
    String add = (request.getParameter("consent") == "y") ? "&prompt=admin_consent" : "";

    // redirect
    String url = authority + "/oauth2/authorize?response_type=code&client_id=" + URLEncoder.encode(clientId, "UTF-8")
      + "&redirect_uri=" + URLEncoder.encode(redirect, "UTF-8") + "&state=" + URLEncoder.encode(state, "UTF-8")
      + "&resource=" + URLEncoder.encode(resource, "UTF-8") + add;
    response.sendRedirect(url);

  }

  public void destroy()
  {
      // do nothing.
  }

}
