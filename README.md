# asp-net-api

TODO

## Features

* TODO

![Alt text](AspNetApi/docs/ingress-routing.png?raw=true "Ingress Routing")

## Dependencies
[.NET 7](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)

[Docker](https://docs.docker.com/engine/install/)

[Minikube](https://minikube.sigs.k8s.io/docs/start/)

[Helm](https://helm.sh/docs/intro/install/)

## API

TODO

## Deploying ASP.NET applications in Kubernetes

ASP.NET WebApi application is deployed in a reverse-proxy environment (Kubernetes). Proxy servers, load balancers, and other network appliances often obscure information about the request before it reaches the app:

-   When HTTPS requests are proxied over HTTP, the original scheme (HTTPS) is lost and must be forwarded in a header.
-   Because an app receives a request from the proxy and not its true source on the Internet or corporate network, the originating client IP address must also be forwarded in a header.

This information may be important in request processing, for example in redirects, authentication, link generation, policy evaluation, and client geolocation.

We'll letting the NGINX ingress controller handle SSL/TLS offloading, so we want to ensure our app uses the correct `X-Forwarded-Proto` headers to understand whether the original request came over HTTP or HTTPS.

![Alt text](AspNetApi/docs/ingress-routing.png?raw=true "Ingress Routing")

### How to configure HTTPS for Ingress
1. **Generate a Self-Signed Certificate:**
Run the following commands to generate a self-signed certificate:
```bash
# Generate a private key and certificate signing request (CSR)
openssl req -newkey rsa:2048 -nodes -keyout tls.key -out tls.csr -subj "/CN=aspnetapi.internal"

# Generate a self-signed certificate from the CSR
openssl x509 -req -days 365 -in tls.csr -signkey tls.key -out tls.crt
```
`"C:\Program Files\Git\usr\bin\openssl"` can be reused from Git.

These commands will create two files: `tls.key` (the private key) and `tls.crt` (the self-signed certificate).

2. **Encode certificate and key:**
To encode the certificate and private key content in base64, you can use the following commands:
```bash
# Encode the certificate
base64 -w 0 -i tls.crt

# Encode the private key
base64 -w 0 -i tls.key
```
`"C:\Program Files\Git\usr\bin\base64"` can be reused from Git.
The `-w 0` option ensures that there are no line breaks in the base64-encoded output.

3. **Create a Kubernetes Secret YAML**
Create your Secret YAML to include the correctly generated base64-encoded content for both the certificate and private key. Your Secret YAML should look like this:
```yaml
apiVersion: v1
kind: Secret
metadata:
  name: mysecret
data:
  tls.crt: |
    <base64-encoded-certificate>
  tls.key: |
    <base64-encoded-private-key>
type: kubernetes.io/tls
```
Replace `<base64-encoded-certificate>` and `<base64-encoded-private-key>` with the content you obtained from the previous step, ensuring that each block is properly indented with spaces.

4. **Apply the Updated Secret:**
Apply the updated Secret configuration to your Kubernetes cluster:
```bash
kubectl apply -f aspnetapi-secret.yaml
```
This will create a Kubernetes Secret named `aspnetapi-secret` with your self-signed certificate and private key.
    

Now you have a Kubernetes Secret containing a self-signed certificate that you can use in your Ingress configuration for testing purposes with the domain "aspnetapi.internal." Remember that self-signed certificates are not trusted by browsers and should not be used in production environments, but they are suitable for testing and development.
### How to configure ASP.NET Core to work with proxy servers and load balancers
`ASPNETCORE_FORWARDEDHEADERS_ENABLED` enables ForwardedHeaders middleware, so the application knows it's behind a reverse-proxy (in this case an NGINX ingress controller).
The middleware updates:

-   [HttpContext.Connection.RemoteIpAddress](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.connectioninfo.remoteipaddress#microsoft-aspnetcore-http-connectioninfo-remoteipaddress): Set using the `X-Forwarded-For` header value. Additional settings influence how the middleware sets `RemoteIpAddress`. For details, see the [Forwarded Headers Middleware options](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/proxy-load-balancer?view=aspnetcore-7.0#forwarded-headers-middleware-options). The consumed values are removed from `X-Forwarded-For`, and the old values are persisted in `X-Original-For`. The same pattern is applied to the other headers, `Host` and `Proto`.
-   [HttpContext.Request.Scheme](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.httprequest.scheme#microsoft-aspnetcore-http-httprequest-scheme): Set using the [`X-Forwarded-Proto`](https://developer.mozilla.org/docs/Web/HTTP/Headers/X-Forwarded-Proto) header value.
-   [HttpContext.Request.Host](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.httprequest.host#microsoft-aspnetcore-http-httprequest-host): Set using the `X-Forwarded-Host` header value.

This option is set in `aspnetapi.yaml`
```yaml
            - name: "ASPNETCORE_FORWARDEDHEADERS_ENABLED"
              value: "true"
```
With this option set `HttpContext` is automatically filled in from X headers:
```bash
HttpContext.Connection.RemoteIpAddress : 192.168.49.2
HttpContext.Connection.RemoteIpPort : 0
HttpContext.Request.Scheme : https
HttpContext.Request.Host : aspnetapi.internal

---Request Headers starting with X
Request-Header X-Request-ID: d20b68596e9eda93ea2f88e601e32b12
Request-Header X-Real-IP: 192.168.49.2
Request-Header X-Original-Proto: http
Request-Header X-Forwarded-Host: aspnetapi.internal
Request-Header X-Forwarded-Port: 443
Request-Header X-Forwarded-Scheme: https
Request-Header X-Scheme: https
Request-Header X-Original-For: [::ffff:10.244.0.115]:35100
```

Without this option X headers are still passed, but `HttpContext` is not automatically updated:
```bash
HttpContext.Connection.RemoteIpAddress : ::ffff:10.244.0.115
HttpContext.Connection.RemoteIpPort : 53636
HttpContext.Request.Scheme : http
HttpContext.Request.Host : aspnetapi.internal

---Request Headers starting with X
Request-Header X-Request-ID: 7bf8a1513ea4f8b34681f4d9e49dbca6
Request-Header X-Real-IP: 192.168.49.2
Request-Header X-Forwarded-For: 192.168.49.2
Request-Header X-Forwarded-Host: aspnetapi.internal
Request-Header X-Forwarded-Port: 443
Request-Header X-Forwarded-Proto: https
Request-Header X-Forwarded-Scheme: https
Request-Header X-Scheme: https
```
## How to run this sample

You can run this sample in 3 different ways:
1. Run .NET app locally.
2. Run in Kubernetes.
3. Using [Helm chart](https://github.com/helm/helm).

### Run local
In **AspNetApi** folder run:
```bash
dotnet run
```
Navigate to [http://localhost:80/](http://localhost/) and [http://localhost:80/swagger](http://localhost/swagger)

![Index page](AspNetApi/docs/swagger-page.png?raw=true "Swagger page")

### Run in Kubernetes
1. **Start Docker**
2. **Navigate to asp-net-api\AspNetApi\charts**
3. **Start Minikube:**
```bash
minikube start
minikube docker-env
minikube -p minikube docker-env --shell powershell | Invoke-Expression

# for cmd:
# @for /f "tokens=*" %i in ('minikube -p minikube docker-env --shell cmd') do @%i
```
4. **Deploy**:
```bash
kubectl apply -f aspnetapi.yaml
kubectl apply -f aspnetapi-service.yaml
kubectl apply -f aspnetapi-ingress.yaml
```
5. **Start Minikube dashboard**:
```bash
minikube dashboard
```
6. **Start Minikube tunnel**:
```bash
minikube tunnel
```
7. **Access minikube VM**:
   
In another command prompt execute this command:
```bash
minikube ssh
```
8. **Access API**:

Access via HTTP:
```bash
curl http://aspnetapi.internal
```
Example output:
```bash
Hello World!---As the application sees it
HttpContext.Connection.RemoteIpAddress : 192.168.49.2
HttpContext.Connection.RemoteIpPort : 0
HttpContext.Request.Scheme : http
HttpContext.Request.Host : aspnetapi.internal

---Request Headers starting with X
Request-Header X-Request-ID: 3b93cfa7f2295e70e504165d1c21b23c
Request-Header X-Real-IP: 192.168.49.2
Request-Header X-Original-Proto: http
Request-Header X-Forwarded-Host: aspnetapi.internal
Request-Header X-Forwarded-Port: 80
Request-Header X-Forwarded-Scheme: http
Request-Header X-Scheme: http
Request-Header X-Original-For: [::ffff:10.244.0.111]:40148
```

And through HTTPS:
```bash
curl --insecure https://aspnetapi.internal
```
`--insecure` option ignores self-signed certificate warning.
Example output:
```bash
Hello World!---As the application sees it
HttpContext.Connection.RemoteIpAddress : 192.168.49.2
HttpContext.Connection.RemoteIpPort : 0
HttpContext.Request.Scheme : https
HttpContext.Request.Host : aspnetapi.internal

---Request Headers starting with X
Request-Header X-Request-ID: 8caadcda122b7344734da850cd493506
Request-Header X-Real-IP: 192.168.49.2
Request-Header X-Original-Proto: http
Request-Header X-Forwarded-Host: aspnetapi.internal
Request-Header X-Forwarded-Port: 443
Request-Header X-Forwarded-Scheme: https
Request-Header X-Scheme: https
Request-Header X-Original-For: [::ffff:10.244.0.111]:40256
```
Note the difference in `X-Forwarded-Port` and `X-Forwarded-Scheme`.
### Using Helm chart

TODO

## References
[Setting environment variables for ASP.NET Core apps in a Helm chart](https://andrewlock.net/deploying-asp-net-core-applications-to-kubernetes-part-5-setting-environment-variables-in-a-helm-chart/)

[Configure ASP.NET Core to work with proxy servers and load balancers](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/proxy-load-balancer)

[How to use Kubernetes Ingress on an ASP.NET Core app](https://www.yogihosting.com/kubernetes-ingress-aspnet-core/)
