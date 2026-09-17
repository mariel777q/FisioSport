# ==========================================
# ETAPA 1: COMPILACIÓN
# ==========================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Copiar el archivo del proyecto
COPY FISIOSPORT/FISIOSPORT.csproj FISIOSPORT/

# Restaurar paquetes NuGet
RUN dotnet restore FISIOSPORT/FISIOSPORT.csproj

# Copiar el código del proyecto
COPY FISIOSPORT/ FISIOSPORT/

# Compilar y publicar
RUN dotnet publish FISIOSPORT/FISIOSPORT.csproj -c Release -o /app/publish --no-restore


# ==========================================
# ETAPA 2: EJECUCIÓN
# ==========================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

# Copiar la aplicación publicada
COPY --from=build /app/publish .

# Render proporciona PORT
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT}

# Iniciar FisioSport
ENTRYPOINT ["dotnet", "FISIOSPORT.dll"]
