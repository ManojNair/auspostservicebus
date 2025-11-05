# Use the official .NET 8 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the project file and restore dependencies
COPY LetsServiceBus.csproj .
RUN dotnet restore "LetsServiceBus.csproj"

# Copy the rest of the application files
COPY . .

# Build the application
RUN dotnet build "LetsServiceBus.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "LetsServiceBus.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Use the official .NET 8 ASP.NET runtime image for running
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Expose port 8080 (ASP.NET Core default port in .NET 8+)
EXPOSE 8080
EXPOSE 8081

# Copy the published application from the publish stage
COPY --from=publish /app/publish .

# Set the entry point for the container
ENTRYPOINT ["dotnet", "LetsServiceBus.dll"]
