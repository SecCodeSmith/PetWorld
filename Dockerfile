# syntax=docker/dockerfile:1

# ---------- build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy only the project files first so `restore` is cached unless they change.
COPY Directory.Build.props PetWorld.sln ./
COPY src/PetWorld.Domain/PetWorld.Domain.csproj           src/PetWorld.Domain/
COPY src/PetWorld.Application/PetWorld.Application.csproj  src/PetWorld.Application/
COPY src/PetWorld.Infrastructure/PetWorld.Infrastructure.csproj src/PetWorld.Infrastructure/
COPY src/PetWorld.Web/PetWorld.Web.csproj                 src/PetWorld.Web/
RUN dotnet restore src/PetWorld.Web/PetWorld.Web.csproj

# Copy the rest of the sources and publish.
COPY . .
RUN dotnet publish src/PetWorld.Web/PetWorld.Web.csproj -c Release -o /app /p:UseAppHost=false

# ---------- runtime stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=5000
EXPOSE 5000
COPY --from=build /app ./
ENTRYPOINT ["dotnet", "PetWorld.Web.dll"]
