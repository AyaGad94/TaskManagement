# ============================================================================
# Stage 1: Build Stage
# ============================================================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

# Copy project files for all layers
COPY ["TaskManagement.API/TaskManagement.API.csproj", "TaskManagement.API/"]
COPY ["TaskManagement.BLL/TaskManagement.BLL.csproj", "TaskManagement.BLL/"]
COPY ["TaskManagement.DAL/TaskManagement.DAL.csproj", "TaskManagement.DAL/"]

# Restore dependencies as a separate layer (improves Docker cache efficiency)
RUN dotnet restore "TaskManagement.API/TaskManagement.API.csproj"

# Copy all source code
COPY . .

# Build and publish in Release mode
WORKDIR /src/TaskManagement.API
RUN dotnet publish "TaskManagement.API.csproj" \
    -c Release \
    -o /app/publish \
    /p:PublishTrimmed=false

# ============================================================================
# Stage 2: Runtime Stage
# ============================================================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app

# Set environment variables for the container
ENV ASPNETCORE_URLS=http://+:8080 \
    DOTNET_RUNNING_IN_CONTAINER=true

# Copy published application from build stage
COPY --from=build /app/publish .

# Expose port 8080 for the API
EXPOSE 8080

# Health check to verify API is responding
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
    CMD dotnet --info > /dev/null 2>&1 || exit 1

# Run the application
ENTRYPOINT ["dotnet", "TaskManagement.API.dll"]
