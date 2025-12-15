# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy all source code
COPY . ./

# Build the application
RUN dotnet build -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=publish /app/publish .

# Create wwwroot directory for static files and uploads
RUN mkdir -p /app/wwwroot/uploads/drivers

# Expose port
EXPOSE 80
EXPOSE 443

ENTRYPOINT ["dotnet", "Nearest.dll"]
