FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app

COPY *.sln ./
COPY SSO.API/SSO.API.csproj ./SSO.API/
COPY SSO.Services/SSO.Services.csproj ./SSO.Services/
COPY SSO.Repository/SSO.Repository.csproj ./SSO.Repository/
COPY SSO.Utility/SSO.Utility.csproj ./SSO.Utility/

RUN dotnet restore

COPY . .

RUN dotnet publish ./SSO.API/SSO.API.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build /app/publish ./

ENV ASPNETCORE_URLS=http://+:${PORT:-5000}
EXPOSE 5000

ENTRYPOINT ["dotnet", "SSO.API.dll"]
