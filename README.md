# Products API DDD

A simple and secure RESTful API for managing products in an e-commerce or inventory system. This API allows users to create, retrieve, and manage products, with authentication implemented via Identity.

## Features

- **CRUD Operations**: Create, Read, Update, Delete products in SQLite.
- **Identity Service**: Secure authentication based on user and roles.
- **Logging**: Centralized logging and Middleware for tracking API requests and errors.
- **Swagger Documentation**: Auto-generated API documentation for easy exploration and testing.
- **DDD**: Domain Driven Design for clean architecture.
- **RabbitMQ**: Messaging for microservices architecture
- **DockerFile to create the image of the application**
  
## Technology Stack

- **.NET 6** (or .NET Core)
- **ASP.NET**
- **Entity Framework Core** (SQLite)
- **Swagger for API documentation**
- **Identity for Authentication**
- **xUnit for unit testing**
- **RabbitMQ for messaging service**
- **Docker to containerize the application**
  
## Prerequisites

Before you can run this project, make sure you have the following installed on your system:

- [.NET SDK 6.0 or later](https://dotnet.microsoft.com/download/dotnet)
- [SQLite](https://www.sqlite.org/)
- A code editor such as [Visual Studio](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)

## Setup Instructions
- In order for the database operations to work, it is necessary to run the **EF migrations**, I included the commands in the entrypoint.sh file
  
- To run the application in a Docker container with HTTPS enabled, 
you must create a certs folder in your project directory containing a valid localhost SSL/TLS certificate (e.g., a .pfx file).
This certificate will be used to secure the communication between your application and clients through HTTPS.

  To generate a **self-signed certificate for local development**, you can use the following command:
  dotnet dev-certs https --trust

Setup the env variables locally or when running the container:
  CERTIFICATE_PATH = /path/your_cert.pfx
  CERTIFICATE_PASSWORD = your_password
  
### Clone the repository

First, clone the repository to your local machine:

```bash
git clone https://github.com/yourusername/products-api.git
cd products-api
