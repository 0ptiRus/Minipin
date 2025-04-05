#!/bin/bash
# Run migrations
dotnet ef database update

# Start the application
dotnet exam_frontend.dll
