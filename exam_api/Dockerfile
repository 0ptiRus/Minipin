# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
EXPOSE 8089
EXPOSE 7279

COPY ../asp_exam.sln ./
COPY ./exam_api/exam_api.csproj ./exam_api/
RUN dotnet restore ./exam_api/exam_api.csproj 
COPY ./exam_api/. ./exam_api/
WORKDIR /app/exam_api
RUN dotnet publish -c Release -o /out

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /out ./
COPY aspnetapp.pfx /https/aspnetapp.pfx
ENV ASPNETCORE_Kestrel__Certificates__Default__Password=strongpass
ENV ASPNETCORE_Kestrel__Certificates__Default__Path=/https/aspnetapp.pfx
ENTRYPOINT ["dotnet", "exam_api.dll"]
