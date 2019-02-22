1. Run 

´
helm install stable/nginx-ingress --namespace ingress-nginx
´


before deploying this API. Otherwise, it won't be accessible from outside the cluster.


2. Configure DNS Name 

Linux:
#!/bin/bash

# Public IP address of your ingress controller
IP="52.174.23.67"

# Name to associate with public IP address
DNSNAME="countingstrings"

# Get the resource-id of the public ip
PUBLICIPID=$(az network public-ip list --query "[?ipAddress!=null]|[?contains(ipAddress, '$IP')].[id]" --output tsv)

# Update public ip address with DNS name
az network public-ip update --ids $PUBLICIPID --dns-name $DNSNAME

Windows: 
az network public-ip update --ids $(az network public-ip list --query "[?ipAddress!=null]|[?contains(ipAddress, '13.80.152.17')].[id]" --output tsv) --dns-name countingstrings
