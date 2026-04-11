FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /build
COPY ["src/BlazorInvest.Web/BlazorInvest.Web.csproj", "src/BlazorInvest.Web/"]
RUN dotnet restore "src/BlazorInvest.Web/BlazorInvest.Web.csproj"
COPY . .
WORKDIR "/build/src/BlazorInvest.Web"
RUN dotnet build "BlazorInvest.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "BlazorInvest.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BlazorInvest.Web.dll"]
