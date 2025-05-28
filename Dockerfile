# Build stage
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app

# Copy the entire solution and restore dependencies
COPY *.sln ./
COPY SSO.API/*.csproj ./SSO.API/
COPY SSO.SERVICES/SSO.Services.csproj ./SSO.SERVICES/
COPY SSO.REPOSITORY/SSO.Repository.csproj ./REPOSITORY/
COPY SSO.Utility/SSO.Utility.csproj ./Utility/

RUN dotnet restore

# Copy everything else
COPY . .

# Publish the Web API project (including all referenced projects)
RUN dotnet publish ./SSO.API/SSO.API.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build /app/publish ./

# Listen on port provided by Render environment variable PORT
ENV ASPNETCORE_URLS=http://+:${PORT:-5000}
EXPOSE 5000

ENTRYPOINT ["dotnet", "SSO.API.dll"]
