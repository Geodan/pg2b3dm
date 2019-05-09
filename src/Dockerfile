FROM mcr.microsoft.com/dotnet/core/sdk:2.2 as build
WORKDIR /src
COPY . .
RUN dotnet build "samples/pg2b3dm/pg2b3dm.csproj" -c Release
RUN dotnet publish "samples/pg2b3dm/pg2b3dm.csproj" -c Release -o /app

FROM mcr.microsoft.com/dotnet/core/runtime:2.2
WORKDIR /app
COPY --from=build /app /app/
ENTRYPOINT ["dotnet", "pg2b3dm.dll"]
