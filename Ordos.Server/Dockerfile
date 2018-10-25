FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY ["Ordos.Server/Ordos.Server.csproj", "Ordos.Server/"]
COPY ["Ordos.Core/Ordos.Core.csproj", "Ordos.Core/"]
COPY ["Ordos.IEDService/Ordos.IEDService.csproj", "Ordos.IEDService/"]
COPY ["Ordos.DataService/Ordos.DataService.csproj", "Ordos.DataService/"]
COPY ["IEC61850.NetCore/IEC61850.NetCore.csproj", "IEC61850.NetCore/"]
RUN dotnet restore "Ordos.Server/Ordos.Server.csproj"
COPY . .
WORKDIR "/src/Ordos.Server"
RUN dotnet build "Ordos.Server.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "Ordos.Server.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Ordos.Server.dll"]