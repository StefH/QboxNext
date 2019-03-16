FROM mcr.microsoft.com/dotnet/core/aspnet:2.2 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build
WORKDIR /src
ARG BuildNumber

COPY ["dependencies/dotnetcore-minimal/src/QboxNext.Core/QboxNext.Core.csproj", "dependencies/dotnetcore-minimal/src/QboxNext.Core/"]
COPY ["dependencies/dotnetcore-minimal/src/QboxNext.Logging/QboxNext.Logging.csproj", "dependencies/dotnetcore-minimal/src/QboxNext.Logging/"]
COPY ["dependencies/dotnetcore-minimal/src/QboxNext.Model/QboxNext.Model.csproj", "dependencies/dotnetcore-minimal/src/QboxNext.Model/"]
COPY ["dependencies/dotnetcore-minimal/src/QboxNext.Qboxes.Parsing/QboxNext.Qboxes.Parsing.csproj", "dependencies/dotnetcore-minimal/src/QboxNext.Qboxes.Parsing/"]
COPY ["dependencies/dotnetcore-minimal/src/QboxNext.Qserver.Core/QboxNext.Qserver.Core.csproj", "dependencies/dotnetcore-minimal/src/QboxNext.Qserver.Core/"]
COPY ["dependencies/dotnetcore-minimal/src/QboxNext.Storage.Abstractions/QboxNext.Storage.Abstractions.csproj", "dependencies/dotnetcore-minimal/src/QboxNext.Storage.Abstractions/"]

COPY ["dependencies/NLog.Extensions.AzureStorage/src/NLog.Extensions.AzureTables/NLog.Extensions.AzureTables.csproj", "dependencies/NLog.Extensions.AzureStorage/src/NLog.Extensions.AzureTables/"]

COPY ["src/QboxNext/QboxNext.csproj", "src/QboxNext/"]
COPY ["src/QboxNext.Extensions/QboxNext.Extensions.csproj", "src/QboxNext.Extensions/"]

COPY ["src-server/QboxNext.Server.Infrastructure.Azure/QboxNext.Server.Infrastructure.Azure.csproj", "src-server/QboxNext.Server.Infrastructure.Azure/"]
COPY ["src-server/QboxNext.Server.DataReceiver/QboxNext.Server.DataReceiver.csproj", "src-server/QboxNext.Server.DataReceiver/"]
COPY ["src-server/QBoxNext.Server.Business/QBoxNext.Server.Business.csproj", "src-server/QBoxNext.Server.Business/"]
COPY ["src-server/QboxNext.Server.Domain/QboxNext.Server.Domain.csproj", "src-server/QboxNext.Server.Domain/"]
COPY ["src-server/QboxNext.Server.Common/QboxNext.Server.Common.csproj", "src-server/QboxNext.Server.Common/"]

RUN dotnet restore "src-server/QboxNext.Server.DataReceiver/QboxNext.Server.DataReceiver.csproj"
COPY . .
WORKDIR "/src/src-server/QboxNext.Server.DataReceiver"
RUN dotnet build "QboxNext.Server.DataReceiver.csproj" -c Release -o /app /p:Version=1.0.0-$BuildNumber

FROM build AS publish
ARG BuildNumber
RUN dotnet publish "QboxNext.Server.DataReceiver.csproj" -c Release -o /app /p:Version=1.0.0-$BuildNumber

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "QboxNext.Server.DataReceiver.dll"]