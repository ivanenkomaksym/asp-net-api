cd AspNetApi\charts

kubectl delete -f aspnetapi.yaml
kubectl delete -f aspnetapi-service.yaml
kubectl delete -f aspnetapi-ingress.yaml

kubectl apply -f aspnetapi.yaml
kubectl apply -f aspnetapi-service.yaml
kubectl apply -f aspnetapi-ingress.yaml

cd ..\..