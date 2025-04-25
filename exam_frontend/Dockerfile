# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
EXPOSE 8083

COPY ../asp_exam.sln ./
COPY ./exam_frontend/exam_frontend.csproj ./exam_frontend/
RUN dotnet restore ./exam_frontend/exam_frontend.csproj

COPY ./exam_frontend/. ./exam_frontend/
WORKDIR /app/exam_frontend
RUN dotnet publish -c Release -o /out

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /out ./
COPY aspnetapp.pfx /https/aspnetapp.pfx
ENV ASPNETCORE_Kestrel__Certificates__Default__Password=strongpass
ENV ASPNETCORE_Kestrel__Certificates__Default__Path=/https/aspnetapp.pfx
ENTRYPOINT ["dotnet", "exam_frontend.dll"]
