FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files first for layer caching
COPY src/Lendora.Domain/Lendora.Domain.csproj src/Lendora.Domain/
COPY src/Lendora.Application/Lendora.Application.csproj src/Lendora.Application/
COPY src/Lendora.Infrastructure/Lendora.Infrastructure.csproj src/Lendora.Infrastructure/
COPY src/Lendora.Api/Lendora.Api.csproj src/Lendora.Api/
COPY nuget.config .
COPY Lendora.slnx .

RUN dotnet restore Lendora.slnx

# Copy everything else and publish
COPY . .
RUN dotnet publish src/Lendora.Api/Lendora.Api.csproj -c Release -o /app/publish --no-restore

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Lendora.Api.dll"]
