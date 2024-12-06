FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

WORKDIR /app

# Copy nuget.config vào container
COPY *.sln ./
COPY GreenGardenClient/*.csproj ./GreenGardenClient/

# Thêm restore với cấu hình timeout
RUN dotnet restore --disable-parallel

COPY . .
RUN dotnet publish GreenGardenClient -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .

ENV ASPNETCORE_URLS="http://+:80"
ENV ASPNETCORE_ENVIRONMENT="Development"
ENV DOTNET_SKIP_FIRST_TIME_EXPERIENCE="true"
ENV DOTNET_USE_POLLING_FILE_WATCHER="true"

EXPOSE 80
ENTRYPOINT ["dotnet", "GreenGardenClient.dll"]
