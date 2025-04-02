# Use the correct base image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy the project file and restore dependencies
COPY ["RentEasy.csproj", "./"]
RUN dotnet restore "RentEasy.csproj"

# Copy everything else and build
COPY . .
RUN dotnet publish "RentEasy.csproj" -c Release -o /app/publish

# Use a smaller runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Expose the required port
EXPOSE 8080
ENTRYPOINT ["dotnet", "RentEasy.dll"]
