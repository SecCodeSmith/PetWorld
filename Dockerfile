# syntax=docker/dockerfile:1

# ---------- Tailwind CSS stage ----------
# Compiles the purged, minified wwwroot/app.css (no runtime CDN).
FROM node:20-alpine AS css
WORKDIR /web
COPY src/PetWorld.Web/package.json src/PetWorld.Web/package-lock.json ./
RUN npm ci
COPY src/PetWorld.Web/tailwind.config.js ./
COPY src/PetWorld.Web/Styles ./Styles
COPY src/PetWorld.Web/Components ./Components
RUN mkdir -p wwwroot && npm run build:css

# ---------- .NET build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy only the project files first so `restore` is cached unless they change.
COPY Directory.Build.props PetWorld.sln ./
COPY src/PetWorld.Domain/PetWorld.Domain.csproj           src/PetWorld.Domain/
COPY src/PetWorld.Application/PetWorld.Application.csproj  src/PetWorld.Application/
COPY src/PetWorld.Infrastructure/PetWorld.Infrastructure.csproj src/PetWorld.Infrastructure/
COPY src/PetWorld.Web/PetWorld.Web.csproj                 src/PetWorld.Web/
RUN dotnet restore src/PetWorld.Web/PetWorld.Web.csproj

# Copy the rest of the sources, drop in the Tailwind-built stylesheet, then publish.
COPY . .
COPY --from=css /web/wwwroot/app.css src/PetWorld.Web/wwwroot/app.css
RUN dotnet publish src/PetWorld.Web/PetWorld.Web.csproj -c Release -o /app /p:UseAppHost=false

# ---------- runtime stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=5000
EXPOSE 5000
COPY --from=build /app ./
ENTRYPOINT ["dotnet", "PetWorld.Web.dll"]
