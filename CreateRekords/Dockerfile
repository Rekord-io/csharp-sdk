# Use the official .NET 8.0 runtime image as the base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# Use the official .NET 8.0 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the entire solution context (including RekordSDK)
COPY . .

# Restore dependencies for the CreateRekords project
RUN dotnet restore "CreateRekords/CreateRekords.csproj"

# Build the application
RUN dotnet build "CreateRekords/CreateRekords.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "CreateRekords/CreateRekords.csproj" -c Release -o /app/publish

# Final stage/image
FROM base AS final
WORKDIR /app

# Copy the published application
COPY --from=publish /app/publish .

# Set the entry point
ENTRYPOINT ["dotnet", "CreateRekords.dll"]


