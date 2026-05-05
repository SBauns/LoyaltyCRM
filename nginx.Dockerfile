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

# 1. Define the Build Argument
ARG API_BASE_URL

# Default value if not provided (optional, prevents build failure)
ENV API_BASE_URL=${API_BASE_URL}

RUN rm /etc/nginx/conf.d/default.conf

COPY nginx.conf /etc/nginx/conf.d/default.conf

# Copy Blazor app wwwroot
COPY --from=webapp-build /app/publish/wwwroot /usr/share/nginx/html

# 2. Inject the Configuration
# We assume your appsettings.json has a placeholder like __API_BASE_URL__
# If you haven't added the placeholder yet, see Step 2 below.
RUN if [ -n "$API_BASE_URL" ]; then \
      sed -i "s|__API_BASE_URL__|$API_BASE_URL|g" /usr/share/nginx/html/appsettings.json; \
    fi

CMD ["nginx", "-g", "daemon off;"]