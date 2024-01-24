# asp-net-api

This repository serves as a comprehensive demonstration of various web service features and best practices. It showcases strategies for optimizing response caching, robust header usage, and security configurations.

## Features

1.  **Response Caching Strategies:** Demonstrates effective response caching strategies, utilizing techniques like `ETag`, `If-None-Match`, and `Cache-Control` headers to improve performance and reduce unnecessary data transfers.
    
2.  **HTTP Security Headers:** Integrates essential HTTP security headers such as Content Security Policy (CSP), X-Content-Type-Options, and X-Frame-Options to enhance the security posture of your web service.
    
3.  **HTTPS Redirection and HSTS:** Implements HTTPS redirection and enforces HTTP Strict Transport Security (HSTS) to ensure secure communication, protecting against potential security threats.
    
4.  **Proxy Server and Load Balancer Configuration:** Provides guidance on configuring [ASP.NET](http://ASP.NET) Core to seamlessly work with proxy servers and load balancers, enabling smooth operation in complex network environments.

5.  **Inject a service into a System.Text.Json converter:** Shows how to add a dummy JSON converter to expose **IServiceProvider** on the **JsonSerializerOptions**. If you want to achieve the same in your integration tests you must manually include **ServiceProviderDummyConverter** in **JsonSerializerOptions** when deserializing from JSON (example in **AspNetApi.IntegrationTests.BasicTests**).

6. **JSON Serialization** using **Newtonsoft** and **System.Text.Json** (example in **AspNetApi.IntegrationTests.JsonConverterTests**).
    
7.  **Containerization with Docker:** Includes Dockerfiles and Docker Compose configurations to containerize the sample microservice, making it easily deployable and scalable.
    
8.  **Kubernetes Deployment:** Offers Kubernetes deployment manifests and Helm charts for streamlined deployment of the microservice to a Kubernetes cluster, simplifying orchestration and management.

## Dependencies
[.NET 7](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)

[Docker](https://docs.docker.com/engine/install/)

[Minikube](https://minikube.sigs.k8s.io/docs/start/)

[Helm](https://helm.sh/docs/intro/install/)

## Caching response

An ETag is a unique identifier that represents the current state of a resource. When a client makes a request to a server for a resource, the server includes an ETag header in the response. The client can then cache the resource and, on subsequent requests, include the ETag in the If-None-Match header.

If the resource hasn’t changed, the server will respond with a 304 Not Modified status code and no body. This allows the client to serve the cached version of the resource, which saves time and bandwidth.

If you forget to include the ETag header in your responses, clients won’t be able to take advantage of this caching optimization. So, make sure to always include ETags in your REST API responses!

One straightforward approach is to compute the ETag just before sending it to the client via the API. The computation method can be as simple as using the LastModified timestamp or generating a hash function based on the response body content.

Since we generate an ETag once the result is prepared, it doesn’t significantly impact the functionality of the API. We still perform a database READ, process it, and prepare a result. However, by not immediately writing the content to the response stream from the Server, we can potentially save the Server’s response bandwidth. This practice is referred to as a “shallow ETag.”

1.  We calculate the ETag string based on the content of the result, but only for successful GET requests (those with a 200 OK response).
2.  In this step, we take the result object and generate the ETag from it using a chosen method, such as hashing or another developer’s choice.
3.  We then examine if there is an ETag included in the request.
4.  If an ETag is found, we compare it to the computed ETag to determine if they match. If they match, it means the data hasn’t been modified.
5.  When the tags match, we respond with a “Not Modified” status.
6.  If the tags don’t match, we include the computed ETag in the response header and allow the response to be sent to the client.
![Alt text](AspNetApi/docs/etag.png?raw=true "If-None-Match and ETag")

Follow these steps to test response caching:
1. In **Postman** or **Swagger** execute **GET http://localhost:80/api/ShoppingCart**
2. Copy the value of **ETag** in response headers
3. In the same request paste copied **ETag** value into new **If-None-Match** request header
4. This time WebApi must respond with empty content and **304 Not Modified** status
5. Now copy the body response received in step 1
6. Execute **PUT http://localhost:80/api/ShoppingCart** and paste the body
7. Modify some content, e.g. change **quantity** to 2
8. Repeat step 3
9. This time the response is different, thus current **ETag** didn't match with provided **If-None-Match** value, so new response with new **ETag** value is returned

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
### Strict-Transport-Security (HSTS)
[HTTP Strict Transport Security (HSTS)](https://cheatsheetseries.owasp.org/cheatsheets/HTTP_Strict_Transport_Security_Cheat_Sheet.html) is an opt-in security enhancement that's specified by a web app through the use of a response header. When a [browser that supports HSTS](https://cheatsheetseries.owasp.org/cheatsheets/Transport_Layer_Protection_Cheat_Sheet.html#browser-support) receives this header:

-   The browser stores configuration for the domain that prevents sending any communication over HTTP. The browser forces all communication over HTTPS.
-   The browser prevents the user from using untrusted or invalid certificates. The browser disables prompts that allow a user to temporarily trust such a certificate.

`UseHsts` isn't recommended in development because the HSTS settings are highly cacheable by browsers. By default, `UseHsts` excludes the local loopback address.
`UseHsts` excludes the following loopback hosts:

-   `localhost` : The IPv4 loopback address.
-   `127.0.0.1` : The IPv4 loopback address.
-   `[::1]` : The IPv6 loopback address.

#### Testing UseHsts Locally
Edit your hosts file: `C:\Windows\System32\drivers\etc\hosts`
```csharp
127.0.0.1 myweb.local
```
Update `launchSettings.json`:
```json
    "https": {
        "commandName": "Project",
        "launchBrowser": true,
        "launchUrl": "swagger",
        "environmentVariables": {
            "ASPNETCORE_ENVIRONMENT": "Development"
        },
        "dotnetRunMessages": true,
        "applicationUrl": "https://myweb.local:7280;http://myweb.local:5072"
    },
```
When executing `https://myweb.local:7280/`
```bash
---Response Headers---
Date: Mon, 25 Sep 2023 12:00:09 GMT
Server: Kestrel
Content-Security-Policy: default-src 'self'
X-Content-Type-Options: nosniff
X-Frame-Options: SAMEORIGIN
X-XSS-Protection: 1; mode=block
Strict-Transport-Security: max-age=2592000
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
---As the application sees it---
HttpContext.Connection.RemoteIpAddress : 192.168.49.2
HttpContext.Connection.RemoteIpPort : 0
HttpContext.Request.Scheme : http
HttpContext.Request.Host : aspnetapi.internal

---Request Headers---
Accept: */*
Host: aspnetapi.internal
User-Agent: curl/7.81.0
X-Request-ID: c3ef8ae5d2b4568298b8b9eca0f5b01a
X-Real-IP: 192.168.49.2
X-Original-Proto: http
X-Forwarded-Host: aspnetapi.internal
X-Forwarded-Port: 80
X-Forwarded-Scheme: http
X-Scheme: http
X-Original-For: [::ffff:10.244.0.115]:54468

---Response Headers---
Date: Mon, 25 Sep 2023 12:44:04 GMT
Server: Kestrel
Transfer-Encoding: chunked
Content-Security-Policy: default-src 'self'
X-Content-Type-Options: nosniff
X-Frame-Options: SAMEORIGIN
X-XSS-Protection: 1; mode=block
```

And through HTTPS:
```bash
curl --insecure https://aspnetapi.internal
```
`--insecure` option ignores self-signed certificate warning.
Example output:
```bash
---As the application sees it---
HttpContext.Connection.RemoteIpAddress : 192.168.49.2
HttpContext.Connection.RemoteIpPort : 0
HttpContext.Request.Scheme : https
HttpContext.Request.Host : aspnetapi.internal

---Request Headers---
Accept: */*
Host: aspnetapi.internal
User-Agent: curl/7.81.0
X-Request-ID: f76b6e123fe553a4c85b4fc276caf506
X-Real-IP: 192.168.49.2
X-Original-Proto: http
X-Forwarded-Host: aspnetapi.internal
X-Forwarded-Port: 443
X-Forwarded-Scheme: https
X-Scheme: https
X-Original-For: [::ffff:10.244.0.115]:54468

---Response Headers---
Date: Mon, 25 Sep 2023 12:44:27 GMT
Server: Kestrel
Transfer-Encoding: chunked
Content-Security-Policy: default-src 'self'
X-Content-Type-Options: nosniff
X-Frame-Options: SAMEORIGIN
X-XSS-Protection: 1; mode=block
```
Note the difference in `X-Forwarded-Port` and `X-Forwarded-Scheme`.

To see header info from Ingress controller, execute
```
curl -I --insecure https://aspnetapi.internal/
```
Example output:
```bash
HTTP/2 405
date: Mon, 25 Sep 2023 12:44:48 GMT
allow: GET
content-security-policy: default-src 'self'
x-content-type-options: nosniff
x-frame-options: SAMEORIGIN
x-xss-protection: 1; mode=block
strict-transport-security: max-age=31536000; includeSubDomains; preload
```
Note `strict-transport-security` added by Ingress, not ASP.NET application itself, according to `annotations` in `aspnetapi=ingress.yaml`:
```yaml
    nginx.ingress.kubernetes.io/configuration-snippet: |
        more_set_headers "Strict-Transport-Security: max-age=31536000; includeSubDomains; preload";
```
### JSON Serialization

The use case is when you need an interface property in your model.

```csharp
public enum BaseType
{
    Base1,
    Base2
}

public interface IBase
{ }

public class Base1 : IBase
{
    public string Value { get; set; }
}

public class Base2 : IBase
{
    public bool Active { get; set; }
}

public class Implementation
{
    public BaseType BaseType;
    public IBase Base { get; set; }
}
```

> In **Newtonsoft** interface property are included by default (check **CanSerializeInterfaceInNewtonsoftButNotInSystemTextJson** unit test).

In **System.Text.Json** If you want your **Base** property to be included in serialization follow this order:
1. Use **[JsonDerivedType]** attribute on your interface and specify all possible derived types in advance
2. Sometimes first approach is not possible in case derived types are spread over different package and you don't want your package with interface to depend on package with derived type. In this case it's then preferred to use  custom **JsonConverter** with implemented **Write** method.

In both **Newtonsoft** and **System.Text.Json** custom **JsonConverter** with implemented **Read** method is only needed when you want to manually deserialize from JSON string, e.g. in unit tests. On web api level this is not required.

### Using Helm chart

TODO

## References
[REST API Caching Best Practices](https://climbtheladder.com/10-rest-api-caching-best-practices/)

[How to build a simple ETag in ASP.NET Core](https://referbruv.com/blog/how-to-build-a-simple-etag-in-aspnet-core/)

[The Differences between Shared and Private Caching](https://hackernoon.com/the-differences-between-shared-and-private-caching)

[Setting environment variables for ASP.NET Core apps in a Helm chart](https://andrewlock.net/deploying-asp-net-core-applications-to-kubernetes-part-5-setting-environment-variables-in-a-helm-chart/)

[Configure ASP.NET Core to work with proxy servers and load balancers](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/proxy-load-balancer)

[How to use Kubernetes Ingress on an ASP.NET Core app](https://www.yogihosting.com/kubernetes-ingress-aspnet-core/)

[Enforce HTTPS in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/enforcing-ssl)

[How to Implement HSTS header in ASP.Net Core 6.0?](https://stackoverflow.com/questions/73376095/how-to-implement-hsts-header-in-asp-net-core-6-0)

[Inject a service into a System.Text.Json converter](https://thomaslevesque.com/2020/08/31/inject-service-into-system-text-json-converter/)
