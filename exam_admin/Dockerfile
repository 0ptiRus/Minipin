# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy the current project (exam_admin) into /src/exam_admin
COPY . exam_admin/

# Copy the exam_frontend project from the parent directory (project root)
COPY ../exam_frontend exam_frontend/

# Restore dependencies for exam_admin
RUN dotnet restore "exam_admin/exam_admin.csproj"

# Build the project
WORKDIR "/src/exam_admin"
RUN dotnet build "exam_admin.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "exam_admin.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "exam_admin.dll"]