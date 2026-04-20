FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY LoyaltyCRM.WebApp/LoyaltyCRM.WebApp.csproj LoyaltyCRM.WebApp/
RUN dotnet restore LoyaltyCRM.WebApp/LoyaltyCRM.WebApp.csproj

COPY . .
WORKDIR /src/LoyaltyCRM.WebApp
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM nginx:alpine AS final
WORKDIR /app
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html

# Copy entrypoint script
# COPY entrypoint.sh /entrypoint.sh
# RUN chmod +x /entrypoint.sh

# Replace default nginx config if needed (optional)
COPY nginx.conf /etc/nginx/conf.d/default.conf

# ENTRYPOINT ["/entrypoint.sh"]
CMD ["nginx", "-g", "daemon off;"]
