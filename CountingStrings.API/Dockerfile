FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY CountingStrings.API/CountingStrings.API.csproj CountingStrings.API/
COPY CountingStrings.API.Contract/CountingStrings.API.Contract.csproj CountingStrings.API.Contract/
COPY CountingStrings.API.Data/CountingStrings.API.Data.csproj CountingStrings.API.Data/
RUN dotnet restore CountingStrings.API/CountingStrings.API.csproj
COPY . .
WORKDIR /src/CountingStrings.API
RUN dotnet build CountingStrings.API.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish CountingStrings.API.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "CountingStrings.API.dll"]
