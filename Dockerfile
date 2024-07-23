FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base

WORKDIR /app

# Copy published application from host
COPY ./publish/. .

# Expose port
EXPOSE 5000
EXPOSE 5001
EXPOSE 80

# Run the application
ENTRYPOINT ["dotnet", "Net.App.Todo.Api.dll"]