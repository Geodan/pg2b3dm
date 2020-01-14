FROM mcr.microsoft.com/dotnet/core/sdk:3.1 as build
WORKDIR /src
COPY . .
RUN dotnet build "pg2b3dm/pg2b3dm.csproj" -c Release
RUN dotnet publish "pg2b3dm/pg2b3dm.csproj" -c Release -o /app

FROM mcr.microsoft.com/dotnet/core/runtime:3.1
WORKDIR /app
COPY --from=build /app /app/
ENTRYPOINT ["dotnet", "pg2b3dm.dll"]
