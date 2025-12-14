
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src


COPY ["EquipmentApi/EquipmentApi.csproj", "EquipmentApi/"]
RUN dotnet restore "EquipmentApi/EquipmentApi.csproj"


COPY . .
WORKDIR "/src/EquipmentApi"
RUN dotnet build "EquipmentApi.csproj" -c Release -o /app/build


FROM build AS publish
RUN dotnet publish "EquipmentApi.csproj" -c Release -o /app/publish


FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EquipmentApi.dll"]