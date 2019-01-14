#Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
#For more information, please see https://aka.ms/containercompat

FROM microsoft/dotnet:2.1-aspnetcore-runtime-nanoserver-1803 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM microsoft/dotnet:2.1-sdk-nanoserver-1803 AS build
WORKDIR /src
COPY ["src/QboxNext.WebApi/QboxNext.WebApi.csproj", "src/QboxNext.WebApi/"]
COPY ["src/QboxNext.Infrastructure.Azure/QboxNext.Infrastructure.Azure.csproj", "src/QboxNext.Infrastructure.Azure/"]
COPY ["src/QboxNext.Domain/QboxNext.Domain.csproj", "src/QboxNext.Domain/"]
COPY ["src/QboxNext.Common/QboxNext.Common.csproj", "src/QboxNext.Common/"]
COPY ["src/QBoxNext.Business/QBoxNext.Business.csproj", "src/QBoxNext.Business/"]
COPY ["src/QboxNext/QboxNext.csproj", "src/QboxNext/"]
COPY ["dependencies/dotnetcore-minimal/QboxNext.Qboxes.Parsing/QboxNext.Qboxes.Parsing.csproj", "dependencies/dotnetcore-minimal/QboxNext.Qboxes.Parsing/"]
COPY ["dependencies/dotnetcore-minimal/Qboxes.Model/Qboxes.Model.csproj", "dependencies/dotnetcore-minimal/Qboxes.Model/"]
COPY ["dependencies/dotnetcore-minimal/QboxNext.Core/QboxNext.Core.csproj", "dependencies/dotnetcore-minimal/QboxNext.Core/"]
COPY ["dependencies/dotnetcore-minimal/QboxNext.Qserver.Core/QboxNext.Qserver.Core.csproj", "dependencies/dotnetcore-minimal/QboxNext.Qserver.Core/"]
RUN dotnet restore "src/QboxNext.WebApi/QboxNext.WebApi.csproj"
COPY . .
WORKDIR "/src/src/QboxNext.WebApi"
RUN dotnet build "QboxNext.WebApi.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "QboxNext.WebApi.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "QboxNext.WebApi.dll"]