# CryptoDashboard
 A little widget of crypto-related price feeds at my fingertips

/api/prices
/api/status


# Dependencies

dotnet add package Newtonsoft.Json --version 10.0.3
dotnet add package AWSSDK.Core --version 3.3.21.11
dotnet add package AWSSDK.CloudWatch --version 3.3.5.1


# Build

dotnet publish -c Release -r linux-x64 -o /api


# Run

dotnet api.dll
