# مرحله اول: ساخت و publish پروژه
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /app

# کپی csproj و بازیابی وابستگی‌ها (restore)
COPY *.csproj ./
RUN dotnet restore

# کپی باقی فایل‌های پروژه و build
COPY . ./
RUN dotnet publish -c Release -o out

# مرحله دوم: اجرا در run-time محیط
FROM mcr.microsoft.com/dotnet/aspnet:5.0
WORKDIR /app
COPY --from=build /app/out .

# پورت اپ را مشخص کن (در صورت نیاز مثلاً 5000)
EXPOSE 80
ENTRYPOINT ["dotnet", "TeamTaskManager.dll"]