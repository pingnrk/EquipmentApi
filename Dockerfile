# 1. ใช้ SDK Image เพื่อ Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy ไฟล์ Project ไป Restore
# ⚠️ เช็คชื่อโฟลเดอร์ให้ตรงกับของคุณ (ถ้าโฟลเดอร์ชื่อ EquipmentApi ก็ตามนี้เลย)
COPY ["EquipmentApi/EquipmentApi.csproj", "EquipmentApi/"]
RUN dotnet restore "EquipmentApi/EquipmentApi.csproj"

# Copy โค้ดที่เหลือแล้ว Build
COPY . .
WORKDIR "/src/EquipmentApi"
RUN dotnet build "EquipmentApi.csproj" -c Release -o /app/build

# Publish เป็นไฟล์ DLL
FROM build AS publish
RUN dotnet publish "EquipmentApi.csproj" -c Release -o /app/publish

# 2. ใช้ Runtime Image เพื่อรันจริง (ขนาดเล็ก)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# เปิด Port 8080 (Render ชอบ Port นี้)
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "EquipmentApi.dll"]