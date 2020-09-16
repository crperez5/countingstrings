# CountingStrings - A Stock-taking App

CountingStrings is a distributed multi-container app hosted in Azure which relies on Docker, Kubertnetes,
.NET Core, Auth0, RabbitMQ and SQL Server.

You can play with the app live at http://countingstrings.centralus.cloudapp.azure.com/

## Project organization

The solution is composed of three apps/services:

### Rest API (CountingStrings.API)
Handles user requests. Sends commands to the Service (OpenSession, CloseSession, SubmitWords, LogRequest, etc).
Retrieves materialized/calculated data from the database and handles it over to the user.

### Service (CountingStrings.Service)
Handles the commands. Stores in the database all the events that happen in the system. Updates Session counters (number of open sessions, number of closed sessions) and amount of requests counter.

### Worker (CountingStrings.Worker)
Background service that wakes up every 30 seconds, checks for new words that have been submitted and performs the heavy calculations (words per session, word frequency).

![alt text](https://i.imgur.com/mXMvWJr.png)

## Built With

* [.netcore](https://dotnet.github.io/) - Web Framework
* [Docker](https://www.docker.com/) - Container Engine
* [AKS](https://azure.microsoft.com/en-us/services/kubernetes-service/) - Azure Kubernetes Service
* [Helms](https://helm.sh/) - A package manager for Kubernetes
* [Auth0](https://auth0.com/) - Authentication and Authorization
* [RabbitMQ](https://www.rabbitmq.com/) - Message Broker
* [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-2017) - Database Server

## Author

* **Cristian Perez Matturro** 

Check out my site: https://crperez.dev
