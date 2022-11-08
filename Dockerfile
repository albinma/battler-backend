FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS build
WORKDIR /app

# disables husly on projet restore
ENV HUSKY=0

# copy csproj and restore as distinct layers
COPY ./src/*/*.csproj ./
RUN for file in $(ls *.csproj); do mkdir -p ${file%.*}/ && mv $file ${file%.*}/; done
RUN dotnet restore Battler.Api

COPY ./src .
RUN dotnet publish Battler.Api -c Release -o out -consoleLoggerParameters:ErrorOnly

FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app

COPY --from=build /app/out .

ENTRYPOINT ["dotnet", "Battler.Api.dll"]

