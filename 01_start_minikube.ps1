minikube start
minikube docker-env
minikube -p minikube docker-env --shell powershell | Invoke-Expression

# for cmd:
# @for /f "tokens=*" %i in ('minikube -p minikube docker-env --shell cmd') do @%i