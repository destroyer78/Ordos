FROM microsoft/dotnet:sdk AS build-env
WORKDIR /app

COPY . ./
RUN dotnet build

EXPOSE 51084