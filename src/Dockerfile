FROM mcr.microsoft.com/dotnet/sdk:5.0 as build
WORKDIR /src
COPY . .
RUN dotnet build "pg2b3dm/pg2b3dm.csproj" -c Release
RUN dotnet publish "pg2b3dm/pg2b3dm.csproj" -c Release -o /app

FROM mcr.microsoft.com/dotnet/runtime:5.0
WORKDIR /app
COPY --from=build /app /app/
RUN apt-get update -yq && \
    apt-get -yq install curl gnupg && \
    curl -sL https://deb.nodesource.com/setup_12.x | bash && \
    apt-get update -yq && \
    apt-get install -yq nodejs && \
    rm -rf /var/lib/apt/lists/*
RUN npm install -g gltf-pipeline
ENTRYPOINT ["dotnet", "pg2b3dm.dll"]
