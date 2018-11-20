FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 51084
ENV ASPNETCORE_URLS=http://+:51084

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY ["Ordos.Server/Ordos.Server.csproj", "Ordos.Server/"]
RUN dotnet restore "Ordos.Server/Ordos.Server.csproj"
COPY . .
WORKDIR "/src/Ordos.Server"
RUN dotnet build "Ordos.Server.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "Ordos.Server.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Ordos.Server.dll", "launch-profile", "ProductionDocker"]
