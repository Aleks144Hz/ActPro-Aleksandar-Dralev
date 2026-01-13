FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy everything from your repo into the docker image
COPY . .

# Restore and Build
RUN dotnet restore "ActPro.sln"
RUN dotnet publish "ActPro.sln" -c Release -o /app/publish

# Run Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Tell Render to use port 8080
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# This starts your app
ENTRYPOINT ["dotnet", "ActPro.dll"]
