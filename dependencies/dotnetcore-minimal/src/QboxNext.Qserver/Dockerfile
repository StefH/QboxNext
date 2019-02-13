# Use Docker image file with .NET Core installed.
FROM microsoft/dotnet:2.2-aspnetcore-runtime

# Set the working directory to /app
WORKDIR /app

# Copy the current directory contents into the container at /app
COPY ./dist/Qserver /app
RUN ls

# Expose port 5000.
EXPOSE 5000

# Set port in environment variable to 5000, Kestrel will be using this for determining which port to listen to.
ENV ASPNETCORE_URLS http://*:5000

# Start Qserver on port 5000
ENTRYPOINT ["dotnet", "QboxNext.Qserver.dll"]
