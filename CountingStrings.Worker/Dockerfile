FROM microsoft/dotnet:2.1-runtime AS base
WORKDIR /app

FROM microsoft/dotnet:2.1-sdk AS cpysrc
WORKDIR /src
COPY CountingStrings.Worker/CountingStrings.Worker.csproj CountingStrings.Worker/
COPY CountingStrings.Service.Data/CountingStrings.Service.Data.csproj CountingStrings.Service.Data/
COPY CountingStrings.API.Contract/CountingStrings.API.Contract.csproj CountingStrings.API.Contract/
COPY CountingStrings.Service/CountingStrings.Service.csproj CountingStrings.Service/

FROM cpysrc AS test
COPY CountingStrings.Worker.Test/CountingStrings.Worker.Test.csproj CountingStrings.Worker.Test/
RUN dotnet restore CountingStrings.Worker.Test/CountingStrings.Worker.Test.csproj
RUN dotnet test

FROM cpysrc AS build
RUN dotnet restore CountingStrings.Worker/CountingStrings.Worker.csproj
COPY . .
WORKDIR /src/CountingStrings.Worker
RUN dotnet build CountingStrings.Worker.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish CountingStrings.Worker.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "CountingStrings.Worker.dll"]
