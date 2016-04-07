<html>
  <head>
    <title>multitenant</title>
    <script src="https://code.jquery.com/jquery-1.12.0.min.js"></script>
    <script src="https://cdnjs.cloudflare.com/ajax/libs/jquery-cookie/1.4.1/jquery.cookie.min.js"></script>
    <script src="index.js"></script>
  </head>
  <body>

    <div>
      <p>Java Authentication Services</p>
      <p>
        <a href="login?consent=y">consent</a>
      </p>
      <div style="margin-top : 15px">
        <a href="login">login</a>
      </p>
    </div>
    <div id="status" style="margin-top : 16px"></div>
    <div style="margin-top : 16px">
      <p>Service Providers</p>
      <p>
        <a id="node-whoami" href="#">Node.js</a>
      </p>
      <p>
        <a id="webapi-whoami" href="#">WebAPI</a>
      </p>
      <p>
        <a id="wcf-whoami" href="#">WCF</a>
      </p>
      <p>
        <a id="func-whoami" href="#">Azure Function</a>
      </p>
    </div>

  </body>
</html>
