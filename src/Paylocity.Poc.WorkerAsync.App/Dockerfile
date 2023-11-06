FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

WORKDIR /src
COPY Paylocity.Poc.WorkerAsync.App.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -r linux-x64 --self-contained false -o /app

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS runtime

WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "Paylocity.Poc.WorkerAsync.App.dll"]
