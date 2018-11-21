FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 51084
ENV ASPNETCORE_URLS=http://+:51084

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY . .
RUN dotnet build "Ordos.Server/Ordos.Server.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "Ordos.Server/Ordos.Server.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Ordos.Server.dll", "launch-profile", "ProductionDocker"]
