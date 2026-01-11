# Etapa de compilación
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
# Copia los archivos de proyecto y restaura dependencias
COPY ["Fundo.Applications.WebApi/Fundo.Applications.WebApi.csproj", "Fundo.Applications.WebApi/"]
RUN dotnet restore "Fundo.Applications.WebApi/Fundo.Applications.WebApi.csproj"

# Copia el resto del código y compila
COPY . .
WORKDIR "/src/Fundo.Applications.WebApi"
RUN dotnet build "Fundo.Applications.WebApi.csproj" -c Release -o /app/build

# Etapa de publicación
FROM build AS publish
RUN dotnet publish "Fundo.Applications.WebApi.csproj" -c Release -o /app/publish

# Etapa final / imagen de ejecución
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Fundo.Applications.WebApi.dll"]