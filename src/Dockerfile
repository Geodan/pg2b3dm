FROM mcr.microsoft.com/dotnet/sdk:6.0 as build
WORKDIR /src
COPY . .
RUN dotnet build "pg2b3dm/pg2b3dm.csproj" -c Release
RUN dotnet publish "pg2b3dm/pg2b3dm.csproj" -c Release -o /app

FROM mcr.microsoft.com/dotnet/runtime:6.0
WORKDIR /app
COPY --from=build /app /app/
ENTRYPOINT ["dotnet", "pg2b3dm.dll"]
