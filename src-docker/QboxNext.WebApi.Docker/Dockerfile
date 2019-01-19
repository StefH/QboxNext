#Depending on the operating system of the host machines(s) that will build or run the containers, the images specified in the FROM statement may need to be changed.
#For more information, please see https://aka.ms/containercompat

FROM microsoft/dotnet:2.1-aspnetcore-runtime-nanoserver-1803 AS base

LABEL maintainer="Stef Heyenrath"

WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM microsoft/dotnet:2.1-sdk-nanoserver-1803 AS build
WORKDIR /work
COPY ["src-docker/QboxNext.WebApi.Docker/QboxNext.WebApi.Docker.csproj", "src-docker/QboxNext.WebApi.Docker/"]
COPY ["src-docker/QboxNext.Business.Docker/QBoxNext.Business.Docker.csproj", "src-docker/QBoxNext.Business.Docker/"]
COPY ["src-webapi/QboxNext.Common/QboxNext.Common.csproj", "src-webapi/QboxNext.Common/"]
COPY ["src-webapi/QboxNext.Domain/QboxNext.Domain.csproj", "src-webapi/QboxNext.Domain/"]
COPY ["src-webapi/QboxNext.Infrastructure.Azure/QboxNext.Infrastructure.Azure.csproj", "src-webapi/QboxNext.Infrastructure.Azure/"]

RUN dotnet restore "src-docker/QboxNext.WebApi.Docker/QboxNext.WebApi.Docker.csproj"

COPY . .
WORKDIR "/work/src-docker/QboxNext.WebApi.Docker"
RUN dotnet build "QboxNext.WebApi.Docker.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "QboxNext.WebApi.Docker.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
EXPOSE 80
ENTRYPOINT ["dotnet", "QboxNext.WebApi.Docker.dll"]

# Just some info to build and run:
# - Build with : docker build -t sheyenrath/qboxnext-webapi-nanoserver-1803 -f .\src-docker\QboxNext.WebApi.Docker\Dockerfile .
# - Run with   : docker run --rm -i -p 5002:80 sheyenrath/qboxnext-webapi-nanoserver-1803