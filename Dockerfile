FROM mcr.microsoft.com/dotnet/sdk:8.0

WORKDIR /src

COPY . .

RUN dotnet restore src/Orders.Api/Orders.Api.csproj
RUN dotnet publish src/Orders.Api/Orders.Api.csproj -c Release -o /app --no-restore

WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080
ENV ConnectionStrings__Default=Server=sql.mrsd-customer.local,1433;Database=Orders;User Id=orders_app;Password=Portal2026!;TrustServerCertificate=True

EXPOSE 80

ENTRYPOINT ["dotnet", "Orders.Api.dll"]
