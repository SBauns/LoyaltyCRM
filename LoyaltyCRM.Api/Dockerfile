FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY LoyaltyCRM.slnx ./
COPY LoyaltyCRM.Api/LoyaltyCRM.Api.csproj LoyaltyCRM.Api/

RUN dotnet restore LoyaltyCRM.Api/LoyaltyCRM.Api.csproj

COPY . .
WORKDIR /src/LoyaltyCRM.Api
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:7272;http://+:5239

ENTRYPOINT ["dotnet", "LoyaltyCRM.Api.dll"]