FROM microsoft/dotnet:2.1-runtime AS base
WORKDIR /app

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY CountingStrings.Service/CountingStrings.Service.csproj CountingStrings.Service/
COPY CountingStrings.API.Contract/CountingStrings.API.Contract.csproj CountingStrings.API.Contract/
COPY CountingStrings.API.Data/CountingStrings.API.Data.csproj CountingStrings.API.Data/
RUN dotnet restore CountingStrings.Service/CountingStrings.Service.csproj
COPY . .
WORKDIR /src/CountingStrings.Service
RUN dotnet build CountingStrings.Service.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish CountingStrings.Service.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "CountingStrings.Service.dll"]
