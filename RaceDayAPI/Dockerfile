
# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Copy project file
COPY ["RaceDayAPI.csproj", "./"]

# Restore dependencies
RUN dotnet restore "RaceDayAPI.csproj"

# Copy the remaining source files
COPY . .

# Publish application
RUN dotnet publish "RaceDayAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Run the application
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

# Application listens on port 8080
ENV ASPNETCORE_HTTP_PORTS=8080

EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "RaceDayAPI.dll"]
