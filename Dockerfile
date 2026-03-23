FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files and restore
COPY ["TicTacToeApp.csproj", "./"]
RUN dotnet restore "TicTacToeApp.csproj"

# Copy the remaining files and build
COPY . .
RUN dotnet publish "TicTacToeApp.csproj" -c Release -o /app/publish

# Run the app
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "TicTacToeApp.dll"]
