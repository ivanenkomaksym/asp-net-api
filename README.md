
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


## How to run this sample

You can run this sample in 3 different ways:
1. Run .NET app locally.
2. Run in Kubernetes.
3. Using [Helm chart](https://github.com/helm/helm).

### Run local

1. In **AspNetApi** folder
```
dotnet run
```
3. Navigate to [http://localhost:80/](http://localhost/) and [http://localhost:80/swagger](http://localhost/swagger)

![Index page](AspNetApi/docs/swagger-page.png?raw=true "Swagger page")

### Run in Kubernetes behind Ingress

ASP.NET WebApi application is deployed in a reverse-proxy environment (Kubernetes). Proxy servers, load balancers, and other network appliances often obscure information about the request before it reaches the app:

-   When HTTPS requests are proxied over HTTP, the original scheme (HTTPS) is lost and must be forwarded in a header.
-   Because an app receives a request from the proxy and not its true source on the Internet or corporate network, the originating client IP address must also be forwarded in a header.

This information may be important in request processing, for example in redirects, authentication, link generation, policy evaluation, and client geolocation.

We'll letting the NGINX ingress controller handle SSL/TLS offloading, so we want to ensure our app uses the correct `X-Forwarded-Proto` headers to understand whether the original request came over HTTP or HTTPS.

![Alt text](AspNetApi/docs/ingress-routing.png?raw=true "Ingress Routing")

1. Start Docker
2. Navigate to **\asp-net-api\AspNetApi\charts**
3. Start Minikube:
```
minikube start
minikube docker-env
minikube -p minikube docker-env --shell powershell | Invoke-Expression

# for cmd:
# @for /f "tokens=*" %i in ('minikube -p minikube docker-env --shell cmd') do @%i
```
4. Deploy:
```
kubectl apply -f aspnetapi.yaml
kubectl apply -f aspnetapi-service.yaml
kubectl apply -f aspnetapi-ingress.yaml
```
5. Start Minikube dashboard:
```
minikube dashboard
```
6. Start Minikube tunnel:
```
minikube tunnel
```
7. In another command prompt ssh to minikube:
```
minikube ssh
```
8. **curl** to API:
```
curl 192.168.49.2
```

### Using Helm chart

TODO

## References
[Setting environment variables for ASP.NET Core apps in a Helm chart](https://andrewlock.net/deploying-asp-net-core-applications-to-kubernetes-part-5-setting-environment-variables-in-a-helm-chart/)

[Configure ASP.NET Core to work with proxy servers and load balancers](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/proxy-load-balancer)
