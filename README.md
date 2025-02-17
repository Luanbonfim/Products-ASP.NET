# Products API DDD

A simple and secure RESTful API for managing products in an e-commerce or inventory system. This API allows users to create, retrieve, and manage products, with authentication implemented via JWT tokens for secure access.

## Features

- **CRUD Operations**: Create, Read, Update, Delete products in SQLite.
- **JWT Authentication**: Secure API with Bearer token authorization.
- **Logging**: Centralized logging and Middleware for tracking API requests and errors.
- **Swagger Documentation**: Auto-generated API documentation for easy exploration and testing.
- **DDD**: Domain Driven Design for clean architecture.
- **Identity Service**: Identity Service to handle users and roles.
  
## Technology Stack

- **.NET 6** (or .NET Core)
- **Entity Framework Core** (SQLite)
- **JWT Authentication**
- **Swagger for API documentation**
- **xUnit for unit testing**

## Prerequisites

Before you can run this project, make sure you have the following installed on your system:

- [.NET SDK 6.0 or later](https://dotnet.microsoft.com/download/dotnet)
- [SQLite](https://www.sqlite.org/)
- A code editor such as [Visual Studio](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)

## Setup Instructions

### Clone the repository

First, clone the repository to your local machine:

```bash
git clone https://github.com/yourusername/products-api.git
cd products-api
