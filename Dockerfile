# ASP.NET Core 9.0 multi-stage Dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY OldPhonePadWeb.csproj ./
RUN dotnet restore "OldPhonePadWeb.csproj"

# Copy everything and build
COPY . .
RUN dotnet publish "OldPhonePadWeb.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "OldPhonePadWeb.dll"]

