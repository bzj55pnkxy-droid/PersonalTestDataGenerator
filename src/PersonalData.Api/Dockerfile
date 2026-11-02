# Build context is the repository root (see compose.yaml).
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /repo
COPY global.json ./
COPY src/PersonalData.Api/PersonalData.Api.csproj src/PersonalData.Api/
RUN dotnet restore src/PersonalData.Api/PersonalData.Api.csproj
COPY src/ src/
RUN dotnet publish src/PersonalData.Api/PersonalData.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "PersonalData.Api.dll"]
