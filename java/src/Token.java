import java.io.*;
import java.util.*;
import java.util.concurrent.*;
import java.net.*;
import javax.servlet.*;
import javax.servlet.http.*;
import org.json.*;
import com.nimbusds.jose.*;
import com.nimbusds.jwt.*;
import com.nimbusds.jose.crypto.*;

import com.microsoft.aad.adal4j.*;
import com.nimbusds.oauth2.sdk.AuthorizationCode;
import com.nimbusds.openid.connect.sdk.AuthenticationErrorResponse;
import com.nimbusds.openid.connect.sdk.AuthenticationResponse;
import com.nimbusds.openid.connect.sdk.AuthenticationResponseParser;
import com.nimbusds.openid.connect.sdk.AuthenticationSuccessResponse;

public class Token extends HttpServlet {

  protected static Properties properties = new Properties();

  public void init() throws ServletException
  {
    try {
      if (properties.size() < 1) {
        InputStream stream = this.getClass().getResourceAsStream("multitenant.properties");
        properties.load(stream);
      }
    }
    catch (Exception ex) {
      ex.printStackTrace();
      throw new ServletException(ex);
    }
  }

  public String getCookie(String name, HttpServletRequest request) {
    javax.servlet.http.Cookie[] cookies = request.getCookies();
    if (cookies != null) {
      for (int i = 0; i < cookies.length; i++) {
        if (cookies[i].getName().equals(name)) {
          return cookies[i].getValue();
        }
      }
    }
    return null;
  }

  public String post(String s_url, String body, String token) throws Throwable {
    URL url = new URL(s_url);
    HttpURLConnection conn = (HttpURLConnection) url.openConnection();
    conn.setDoOutput(true);
    conn.setInstanceFollowRedirects(false);
    conn.setRequestMethod("POST");
    conn.setRequestProperty("Content-Type", "application/json");
    conn.setRequestProperty("Accept", "application/json");
    conn.setRequestProperty("Authorization", "bearer " + token);
    conn.setUseCaches(false);
    try( DataOutputStream wr = new DataOutputStream(conn.getOutputStream()) ) {
      byte[] buffer = body.getBytes("UTF-8");
      wr.write(buffer);
    }
    try( Reader in = new BufferedReader(new InputStreamReader(conn.getInputStream(), "UTF-8")) ) {
      StringBuilder sb = new StringBuilder();
      for (int c; (c = in.read()) >= 0;) {
        sb.append((char)c);
      }
      String result = sb.toString();
      return result;
    }
  }

  static public String join(String delimiter, List<String> list)
  {
    StringBuilder sb = new StringBuilder();
    boolean first = true;
    for (String item : list)
    {
      if (first) {
        first = false;
      } else {
        sb.append(delimiter);
      }
      sb.append(item);
    }
    return sb.toString();
  }

  public void doGet(HttpServletRequest request, HttpServletResponse response) throws ServletException, IOException
  {

      // variables
      String authority    = (String) properties.get("login.authority");
      String clientId     = (String) properties.get("login.clientId");
      String clientSecret = (String) properties.get("token.clientSecret");
      String redirect     = (String) properties.get("login.redirect");
      String state        = (String) properties.get("login.state");
      String resource     = (String) properties.get("login.resource");
      String key          = (String) properties.get("token.key");
      String homepage     = (String) properties.get("token.homepage");

      // ensure this is part of the same authorization chain
      String state_qs = request.getParameter("state");
      String state_c = getCookie("authstate", request);
      if (!state_qs.equals(state_c)) {
        response.sendError(400, "Bad Request: this does not appear to be part of the same authorization chain (" + state_qs + ", " + state_c + ").");
      } else {

        // use the executor service
        ExecutorService service = null;
        try {

          // create the context
          service = Executors.newFixedThreadPool(1);
          AuthenticationContext authContext = new AuthenticationContext(authority, true, service);

          // START acquire a token from the code
          Future<AuthenticationResult> promise = authContext.acquireTokenByAuthorizationCode(
            request.getParameter("code"),
            new URI(redirect),
            new ClientCredential(clientId, clientSecret),
            resource,
            null);

          // COMPLETE acquire a token from the code
          AuthenticationResult result = promise.get();
          if (result == null) {
            response.sendError(401, "no auth result");
          } else {

            // get the group membership
            String userId = result.getUserInfo().getDisplayableId();
            String domain = userId.split("@")[1];
            String url1 = "https://graph.windows.net/" + domain + "/users/" + userId + "/getMemberGroups?api-version=1.6";
            String s_groups = post(url1, "{ \"securityEnabledOnly\": false }", result.getAccessToken());
            JSONObject j_groups = new JSONObject(s_groups);
            JSONArray a_groups = j_groups.getJSONArray("value");
            List<String> groupIds = new ArrayList<String>();
            for (int i = 0; i < a_groups.length(); i++) {
              groupIds.add(a_groups.getString(i));
            }

            // get the names of all groups the user is a member of
            String url2 = "https://graph.windows.net/" + domain + "/getObjectsByObjectIds?api-version=1.6";
            String s_details = post(url2, "{ \"objectIds\": [\"" + join("\",\"", groupIds) + "\"], \"types\": [ \"group\" ] }", result.getAccessToken());
            JSONObject j_details = new JSONObject(s_details);
            JSONArray a_details = j_details.getJSONArray("value");
            List<String> groupNames = new ArrayList<String>();
            for (int i = 0; i < a_details.length(); i++) {
              JSONObject j_detail = a_details.getJSONObject(i);
              String s_detail = j_detail.getString("displayName");
              if (s_detail.startsWith("testauth_")) {
                groupNames.add(s_detail.replaceFirst("testauth_", ""));
              }
            }

            // define rights
            List<String> rights = new ArrayList<String>();
            if (groupNames.contains("admins")) {
              rights.add("can admin");
              rights.add("can edit");
              rights.add("can view");
            } else if (groupNames.contains("users")) {
              rights.add("can view");
            }

            // build the JWT
            JWTClaimsSet claimsSet = new JWTClaimsSet();
            claimsSet.setIssuer("http://testauth.plasne.com");
            claimsSet.setSubject(userId);
            claimsSet.setCustomClaim("scope", "[\"" + join("\",\"", groupNames) + "\"]");
            claimsSet.setCustomClaim("rights", "[\"" + join("\",\"", rights) + "\"]");
            claimsSet.setExpirationTime(new Date(new Date().getTime() + 4 * 60 * 1000)); // 4 hours
            SignedJWT signedJWT = new SignedJWT(new JWSHeader(JWSAlgorithm.HS256), claimsSet);
            signedJWT.sign(new MACSigner(key));
            String jwt = signedJWT.serialize();

            // redirect
            response.sendRedirect(homepage + "?accessToken=" + jwt);

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
