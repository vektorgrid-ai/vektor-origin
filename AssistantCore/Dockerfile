FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
# Ensure the ASP.NET app listens on the same port that compose and the app expect
ENV ASPNETCORE_URLS=http://+:5264

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["AssistantCore/AssistantCore.csproj", "AssistantCore/"]
RUN dotnet restore "AssistantCore/AssistantCore.csproj"
COPY . .
WORKDIR "/src/AssistantCore"
RUN dotnet build "./AssistantCore.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./AssistantCore.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
ENV MINIMUM_LOG_LEVEL=Information
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 5264
ENTRYPOINT ["dotnet", "AssistantCore.dll"]
