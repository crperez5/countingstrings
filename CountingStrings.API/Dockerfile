FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY CountingStrings.API/CountingStrings.API.csproj CountingStrings.API/
COPY CountingStrings.API.DataAccess/CountingStrings.API.DataAccess.csproj CountingStrings.API.DataAccess/
COPY CountingStrings.API.Bus/CountingStrings.API.Bus.csproj CountingStrings.API.Bus/
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
