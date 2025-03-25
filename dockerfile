# Step 1: Set the base image (runtime image)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 7263

# Step 2: Set up the build image
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Step 3: Copy the solution file and restore dependencies
COPY ["Products.sln", "./"]

# Step 4: Copy all project files to the container (this includes source code, configuration, etc.)
COPY . .

# Copy the certificate into the container
COPY certs/api_cert.pfx /certs/api_cert.pfx

# Step 5: Restore dependencies using dotnet restore
RUN dotnet restore Products.sln

# Step 6: Build the solution
RUN dotnet build Products.sln -c Release -o /app/build

# Step 7: Publish the application
FROM build AS publish
RUN dotnet publish Products.sln -c Release -o /app/publish

# Step 8: Set up the runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=build /certs /certs

# Create volume for database
VOLUME /app/data

ENTRYPOINT ["dotnet", "ProductsAPI.dll"]
