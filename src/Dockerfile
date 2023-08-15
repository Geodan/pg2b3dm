FROM mcr.microsoft.com/dotnet/sdk:8.0-preview as build
WORKDIR /src
COPY . .
RUN dotnet build "pg2b3dm/pg2b3dm.csproj" -c Release
RUN dotnet publish "pg2b3dm/pg2b3dm.csproj" -c Release -o /app

FROM mcr.microsoft.com/dotnet/runtime:8.0-preview
WORKDIR /app
COPY --from=build /app /app/
ENTRYPOINT ["dotnet", "pg2b3dm.dll"]
