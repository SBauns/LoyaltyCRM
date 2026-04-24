FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy only csproj files first
COPY LoyaltyCRM.WebApp/LoyaltyCRM.WebApp.csproj LoyaltyCRM.WebApp/
COPY LoyaltyCRM.DTOs/LoyaltyCRM.DTOs.csproj LoyaltyCRM.DTOs/

# Restore
RUN dotnet restore LoyaltyCRM.WebApp/LoyaltyCRM.WebApp.csproj

# Copy the rest of the source
COPY . .

# Publish
WORKDIR /src/LoyaltyCRM.WebApp
RUN dotnet publish -c Release -o /app/publish

# Final stage
FROM nginx:alpine AS final

RUN rm /etc/nginx/conf.d/default.conf

COPY LoyaltyCRM.WebApp/nginx.conf /etc/nginx/conf.d/default.conf

# Copy published WASM files
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html

# Copy staticwebassets manifest file
COPY --from=build /app/publish/LoyaltyCRM.WebApp.staticwebassets.endpoints.json /usr/share/nginx/html/

# Copy dotnet.js to _framework directory
COPY --from=build /app/publish/dotnet.js /usr/share/nginx/html/_framework/

# Fix for blazor.webassembly.js wildcard
RUN cd /usr/share/nginx/html/_framework && \
    ln -sf blazor.webassembly.*.js blazor.webassembly.js && \
    ln -sf dotnet.runtime.*.js dotnet.runtime.js && \
    ln -sf dotnet.native.*.js dotnet.native.js && \
    ln -sf dotnet.native.*.wasm dotnet.native.wasm && \
    ln -sf dotnet.runtime.*.js dotnet.boot.js

CMD ["nginx", "-g", "daemon off;"]
