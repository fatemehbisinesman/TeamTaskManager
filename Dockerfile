# مرحله Base: اجرای برنامه
FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 10000

# مرحله Build: ساخت و پابلیش پروژه
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "TeamTaskManager.csproj"
RUN dotnet publish "TeamTaskManager.csproj" -c Release -o /app/publish

# مرحله Final: اجرای برنامه پابلیش‌شده
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
CMD ["dotnet", "TeamTaskManager.dll"]