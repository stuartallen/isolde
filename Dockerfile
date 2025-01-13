# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /source

# Copy csproj and restore dependencies
COPY src/*.csproj ./src/
RUN dotnet restore ./src/

# Copy everything else and build
COPY src/. ./src/
RUN dotnet build ./src/ --configuration Release
RUN dotnet publish ./src/ --configuration Release --no-build -o /app

# Runtime stage
FROM mcr.microsoft.com/dotnet/runtime:9.0
WORKDIR /app
COPY --from=build /app .
COPY --from=build /source/src/text/. ./text/
ENTRYPOINT ["dotnet", "isolde.dll"]