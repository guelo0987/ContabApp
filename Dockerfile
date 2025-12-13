# Use the official .NET 9 SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy the solution file and project file(s)
COPY ["ContabBack.sln", "./"]
COPY ["ContabBackApp/ContabBackApp.csproj", "ContabBackApp/"]

# Restore dependencies
RUN dotnet restore

# Copy the rest of the source code
COPY . .

# Build and publish the application
WORKDIR "/src/ContabBackApp"
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Use the official .NET 9 runtime image for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Configure for Render
# Render sets a PORT environment variable, but .NET listens on 8080 by default.
# We map simple port usage or ensure we listen on all interfaces.
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "ContabBackApp.dll"]
