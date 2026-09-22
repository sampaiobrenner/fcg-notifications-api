FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY global.json nuget.config Directory.Build.props Directory.Build.targets Directory.Packages.props ./
COPY src/Fcg.Notifications.Domain/Fcg.Notifications.Domain.csproj src/Fcg.Notifications.Domain/
COPY src/Fcg.Notifications.Application/Fcg.Notifications.Application.csproj src/Fcg.Notifications.Application/
COPY src/Fcg.Notifications.Infrastructure/Fcg.Notifications.Infrastructure.csproj src/Fcg.Notifications.Infrastructure/
COPY src/Fcg.Notifications.WebApi/Fcg.Notifications.WebApi.csproj src/Fcg.Notifications.WebApi/
RUN dotnet restore src/Fcg.Notifications.WebApi/Fcg.Notifications.WebApi.csproj

COPY src/ src/
RUN dotnet publish src/Fcg.Notifications.WebApi/Fcg.Notifications.WebApi.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "Fcg.Notifications.WebApi.dll"]
