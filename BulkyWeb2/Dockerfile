FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["BulkyWeb2/BulkyWeb2.csproj", "BulkyWeb2/"]
RUN dotnet restore "BulkyWeb2/BulkyWeb2.csproj"
COPY . .
WORKDIR "/src/BulkyWeb2"
RUN dotnet build "BulkyWeb2.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BulkyWeb2.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN mkdir -p /app/wwwroot/images/product && \
    chmod -R 755 /app/wwwroot/images
ENTRYPOINT ["dotnet", "BulkyWeb2.dll"] 