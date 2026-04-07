# Create the solution
dotnet new sln -n LoyaltyCRM

# Create the Web API project
dotnet new webapi -n LoyaltyCRM.Api

# Create the WASM project
dotnet new blazorwasm -n LoyaltyCRM.WebApp

# Create class library projects
dotnet new classlib -n LoyaltyCRM.Domain
dotnet new classlib -n LoyaltyCRM.DTOs
dotnet new classlib -n LoyaltyCRM.Infrastructure
dotnet new classlib -n LoyaltyCRM.Services

# Create test project (xUnit)
dotnet new xunit -n LoyaltyCRM.Tests

# Add projects to the solution
dotnet sln add LoyaltyCRM.Api/LoyaltyCRM.Api.csproj
dotnet sln add LoyaltyCRM.WebApp/LoyaltyCRM.WebApp.csproj
dotnet sln add LoyaltyCRM.Domain/LoyaltyCRM.Domain.csproj
dotnet sln add LoyaltyCRM.DTOs/LoyaltyCRM.DTOs.csproj
dotnet sln add LoyaltyCRM.Infrastructure/LoyaltyCRM.Infrastructure.csproj
dotnet sln add LoyaltyCRM.Services/LoyaltyCRM.Services.csproj
dotnet sln add LoyaltyCRM.Tests/LoyaltyCRM.Tests.csproj

# Add project references
# API depends on Services + DTOs
dotnet add LoyaltyCRM.Api reference LoyaltyCRM.Services
dotnet add LoyaltyCRM.Api reference LoyaltyCRM.DTOs

# WebApp depends on DTOs
dotnet add LoyaltyCRM.WebApp reference LoyaltyCRM.DTOs

# Services depend on Domain + Infrastructure + DTOs
dotnet add LoyaltyCRM.Services reference LoyaltyCRM.Domain
dotnet add LoyaltyCRM.Services reference LoyaltyCRM.Infrastructure
dotnet add LoyaltyCRM.Services reference LoyaltyCRM.DTOs

# Infrastructure depends on Domain
dotnet add LoyaltyCRM.Infrastructure reference LoyaltyCRM.Domain

# Tests depend on Services, Domain, DTOs, and API (optional)
dotnet add LoyaltyCRM.Tests reference LoyaltyCRM.Services
dotnet add LoyaltyCRM.Tests reference LoyaltyCRM.Domain
dotnet add LoyaltyCRM.Tests reference LoyaltyCRM.DTOs
dotnet add LoyaltyCRM.Tests reference LoyaltyCRM.Api

# Add common NuGet packages
# Entity Framework Core + SQL Server
dotnet add LoyaltyCRM.Infrastructure package Microsoft.EntityFrameworkCore
dotnet add LoyaltyCRM.Infrastructure package Microsoft.EntityFrameworkCore.SqlServer
dotnet add LoyaltyCRM.Infrastructure package Microsoft.EntityFrameworkCore.Design
dotnet add LoyaltyCRM.Infrastructure package Microsoft.AspNetCore.Identity.EntityFrameworkCore
# dotnet add LoyaltyCRM.Api package Microsoft.AspNetCore.Identity.EntityFrameworkCore

# Add Configuration to Infrastructure
dotnet add LoyaltyCRM.Infrastructure package Microsoft.Extensions.Configuration
dotnet add LoyaltyCRM.Infrastructure package Microsoft.Extensions.Configuration.Json
dotnet add LoyaltyCRM.Infrastructure package Microsoft.Extensions.Configuration.FileExtensions

# Authentication & Authorization (JWT)
dotnet add LoyaltyCRM.Api package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add LoyaltyCRM.Domain package Microsoft.AspNetCore.Identity.EntityFrameworkCore

# FluentValidation (optional but recommended)
# dotnet add LoyaltyCRM.Services package FluentValidation
# dotnet add LoyaltyCRM.Api package FluentValidation.AspNetCore

# Test project packages
dotnet add LoyaltyCRM.Tests package Moq
dotnet add LoyaltyCRM.Tests package FluentAssertions
dotnet add LoyaltyCRM.Tests package Microsoft.AspNetCore.Mvc.Testing