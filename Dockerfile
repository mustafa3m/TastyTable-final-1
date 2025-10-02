# build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copy solution and restore
COPY TastyTable.sln ./
COPY Directory.Build.props ./
COPY TastyTable.Core/TastyTable.Core.csproj TastyTable.Core/
COPY TastyTable.Data/TastyTable.Data.csproj TastyTable.Data/
COPY TastyTable.Services/TastyTable.Services.csproj TastyTable.Services/
COPY TastyTable.Api/TastyTable.Api.csproj TastyTable.Api/
COPY TastyTable.Tests/TastyTable.Tests.csproj TastyTable.Tests/
RUN dotnet restore TastyTable.Api/TastyTable.Api.csproj

# copy everything and publish
COPY . .
RUN dotnet publish TastyTable.Api/TastyTable.Api.csproj -c Release -o /app/publish

# runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80

# Tell ASP.NET Core to bind to port 80
ENV ASPNETCORE_URLS=http://+:80

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TastyTable.Api.dll"]
