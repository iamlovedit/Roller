FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Roller.Tutorial/Roller.Tutorial.csproj", "Roller.Tutorial/"]
COPY ["Infrastructure/Infrastructure.csproj", "Infrastructure/"]
COPY ["Infrastructure.EventBus/Infrastructure.EventBus.csproj", "Infrastructure.EventBus/"]
RUN dotnet restore "Roller.Tutorial/Roller.Tutorial.csproj"
COPY . .
WORKDIR "/src/Roller.Tutorial"
RUN dotnet build "Roller.Tutorial.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Roller.Tutorial.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Roller.Tutorial.dll"]
