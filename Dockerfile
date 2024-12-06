FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app
COPY *.sln ./
COPY GreenGardenClient/*.csproj ./GreenGardenClient/
RUN dotnet restore --disable-parallel
COPY . .
RUN dotnet publish GreenGardenClient -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .
EXPOSE 80
ENTRYPOINT ["dotnet", "GreenGardenClient.dll"]
