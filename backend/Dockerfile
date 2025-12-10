FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

COPY . .
RUN dotnet restore
RUN dotnet build -c Release

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app .

ENTRYPOINT ["sh", "-c", "dotnet ef database update && dotnet backend.dll"]
