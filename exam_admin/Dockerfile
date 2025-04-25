# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
EXPOSE 8085
EXPOSE 7117
 
COPY ../asp_exam.sln ./
COPY ./exam_admin/exam_admin.csproj ./exam_admin/
RUN dotnet restore ./exam_admin/exam_admin.csproj

COPY ./exam_admin/. ./exam_admin/
WORKDIR /app/exam_admin
RUN dotnet publish -c Release -o /out

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /out ./
COPY aspnetapp.pfx /https/aspnetapp.pfx
ENV ASPNETCORE_Kestrel__Certificates__Default__Password=strongpass
ENV ASPNETCORE_Kestrel__Certificates__Default__Path=/https/aspnetapp.pfx
ENTRYPOINT ["dotnet", "exam_admin.dll"]
