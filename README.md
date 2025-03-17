# Products API DDD

A simple and secure RESTful API for managing products in an e-commerce or inventory system. This API allows users to create, retrieve, and manage products, with authentication implemented via Identity.

## Features

- **CRUD Operations**: Create, Read, Update, Delete products in SQLite.
- **Identity Service**: Secure authentication based on user and roles.
- **Logging**: Centralized logging and Middleware for tracking API requests and errors.
- **Swagger Documentation**: Auto-generated API documentation for easy exploration and testing.
- **DDD**: Domain Driven Design for clean architecture.
- **RabbitMQ**: Messaging for microservices architecture.
- **Google oAuth**: authentication through google oAuth.
- **Role based authorizarion** Only admin users can run write operations
  
## Technology Stack

- **.NET 6** (or .NET Core)
- **ASP.NET**
- **Entity Framework Core** (SQLite)
- **Swagger for API documentation**
- **Identity for Authentication**
- **xUnit for unit testing**
- **RabbitMQ for messaging service**
- **DockerFile to containerize the application**
- **oAuth 2.0 to allow third party login**

  ![image](https://github.com/user-attachments/assets/e678eb4f-68d1-45a5-869c-975acc199c6d)

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
