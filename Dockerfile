# Use the .NET 9.0 SDK to build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy everything
COPY . .

# Restore and Build using .NET 9
RUN dotnet restore "ActPro.sln"
RUN dotnet publish "ActPro.sln" -c Release -o /app/publish

# Use the .NET 9.0 Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .

# Set Render Port
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENV ConnectionStrings__DefaultConnection="Data Source=SQL8020.site4now.net;Initial Catalog=db_ac3bcb_actpro;User Id=db_ac3bcb_actpro_admin;Password=SmartPro#12;TrustServerCertificate=True;"

ENTRYPOINT ["dotnet", "ActPro.dll"]
