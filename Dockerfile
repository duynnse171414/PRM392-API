# Bước 1: Dùng image SDK để build app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy toàn bộ solution
COPY . .

# Restore các dependency (đường dẫn theo đúng vị trí .csproj)
RUN dotnet restore "MyApp.Api/MyApp.Api.csproj"

# Build và publish
RUN dotnet publish "MyApp.Api/MyApp.Api.csproj" -c Release -o /app/publish

# Bước 2: Dùng image runtime để chạy app
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Render sẽ truyền PORT động; không cố định cổng tại đây
# (Program.cs đã đọc PORT và bind)

ENTRYPOINT ["dotnet", "MyApp.Api.dll"]
