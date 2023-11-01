FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Post.Query/Post.Query.Api/Post.Query.Api.csproj", "Post.Query/Post.Query.Api/"]
COPY ["../CQRS/CQRS.Core/CQRS.Core.csproj", "../CQRS/CQRS.Core/"]
COPY ["Post.Query/Post.Query.Domain/Post.Query.Domain.csproj", "Post.Query/Post.Query.Domain/"]
COPY ["Post.Common/Post.Common.csproj", "Post.Common/"]
COPY ["Post.Query/Post.Query.Infrastructure/Post.Query.Infrastructure.csproj", "Post.Query/Post.Query.Infrastructure/"]
RUN dotnet restore "Post.Query/Post.Query.Api/Post.Query.Api.csproj"
COPY . .
WORKDIR "/src/Post.Query/Post.Query.Api"
RUN dotnet build "Post.Query.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Post.Query.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Post.Query.Api.dll"]
