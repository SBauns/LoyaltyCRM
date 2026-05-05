# Build Blazor WebApp
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS webapp-build
WORKDIR /src

COPY LoyaltyCRM.WebApp/LoyaltyCRM.WebApp.csproj LoyaltyCRM.WebApp/
COPY LoyaltyCRM.DTOs/LoyaltyCRM.DTOs.csproj LoyaltyCRM.DTOs/

RUN dotnet restore LoyaltyCRM.WebApp/LoyaltyCRM.WebApp.csproj

COPY . .

WORKDIR /src/LoyaltyCRM.WebApp
RUN dotnet publish -c Release -o /app/publish

# Final nginx reverse proxy and static file server
FROM nginx:alpine

RUN rm /etc/nginx/conf.d/default.conf

COPY nginx.conf /etc/nginx/conf.d/default.conf

# Copy Blazor app wwwroot
COPY --from=webapp-build /app/publish/wwwroot /usr/share/nginx/html

CMD ["nginx", "-g", "daemon off;"]
