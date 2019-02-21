FROM microsoft/dotnet:2.1-sdk as builder
WORKDIR /src
COPY CountingStrings.Worker/CountingStrings.Worker.csproj CountingStrings.Worker/
COPY CountingStrings.Service.Data/CountingStrings.Service.Data.csproj CountingStrings.Service.Data/
COPY CountingStrings.API.Contract/CountingStrings.API.Contract.csproj CountingStrings.API.Contract/
COPY CountingStrings.Service/CountingStrings.Service.csproj CountingStrings.Service/
COPY CountingStrings.Worker.Test/CountingStrings.Worker.Test.csproj CountingStrings.Worker.Test/
RUN dotnet restore CountingStrings.Worker.Test/CountingStrings.Worker.Test.csproj
COPY . .
