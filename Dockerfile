# Etapa 1: Compilación
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY FISIOSPORT/FISIOSPORT/FISIOSPORT.csproj FISIOSPORT/

RUN dotnet restore FISIOSPORT/FISIOSPORT.csproj

COPY FISIOSPORT/FISIOSPORT/ FISIOSPORT/

RUN dotnet publish FISIOSPORT/FISIOSPORT/FISIOSPORT.csproj -c Release -o /app/publish --no-restore


# Etapa 2: Ejecución
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT}

ENTRYPOINT ["dotnet", "FISIOSPORT.dll"]
