# Use Docker image file with .NET Core installed.
FROM microsoft/dotnet:2.2-aspnetcore-runtime

# Set the working directory to /app
WORKDIR /app

# Copy the current directory contents into the container at /app
COPY ./dist/Qservice /app
RUN ls

# Expose port 5002.
EXPOSE 5002

# Start Qservice
ENTRYPOINT ["dotnet", "QboxNext.Qservice.dll"]
