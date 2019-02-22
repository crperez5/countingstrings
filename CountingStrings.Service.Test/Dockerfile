FROM microsoft/dotnet:2.1-sdk as builder
WORKDIR /src
COPY CountingStrings.Service.Data/CountingStrings.Service.Data.csproj CountingStrings.Service.Data/
COPY CountingStrings.API.Contract/CountingStrings.API.Contract.csproj CountingStrings.API.Contract/
COPY CountingStrings.Service/CountingStrings.Service.csproj CountingStrings.Service/
COPY CountingStrings.Service.Test/CountingStrings.Service.Test.csproj CountingStrings.Service.Test/
RUN dotnet restore CountingStrings.Service.Test/CountingStrings.Service.Test.csproj
COPY . .
