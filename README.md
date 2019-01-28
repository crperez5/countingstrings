# CountingStrings - A Stock-taking App

CountingStrings is a distributed multi-container app hosted in Azure which relies on Docker Containers,
.NET Core, RabbitMQ and SQL Server.

You can play with the app live at https://countingstrings.azurewebsites.net/swagger/index.html

## Running the app locally

Download the repository. 
Navigate to Docker folder inside the root folder.
Open a PowerShell window and enter the command:

```
docker-compose up
```

Notice: the first time you run this command, it may take it a few minutes since Docker has to pull the images from the web and start the containers. When this is done, you should see a log similar to: "Application is listening on Port 80".
Then you will be able to browse http://localhost:8000/swagger/index.html and start playing with the app.

### Prerequisites

You need to have Docker installed in your machine.

## Running the tests

The projects CountingStrings.Service.Test and CountingStrings.Worker.Test implement tests for the most important workflows. These are xunit tests that can be run from Visual Studio Test Explorer or directly from the command line. The tests rely on EFCore and Migrations to spawn a fresh localdb database. For each test, the db gets populated with the required data. Once the test is completed, the database is deleted.

## Built With

* [.netcore](https://dotnet.github.io/) - Web Framework
* [Docker](https://www.docker.com/) - Container Engine
* [RabbitMQ](https://www.rabbitmq.com/) - Message Broker
* [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-2017) - Database Server

## Author

* **Cristian Perez Matturro** 

Check out my linkedin page: https://www.linkedin.com/in/cristianperezmatturro/) 
