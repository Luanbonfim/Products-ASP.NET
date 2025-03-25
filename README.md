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
- [Docker](https://www.docker.com/products/docker-desktop) 
- A code editor such as [Visual Studio](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)

## Setup Instructions

### Clone the repository

First, clone the repository to your local machine:

```bash
git clone https://github.com/yourusername/products-api.git
cd products-api
```

### Environment Setup

1. Generate an HTTPS certificate for development:

```bash
dotnet dev-certs https -ep certs/api_cert.pfx -p your_certificate_password
```

2. Set up environment variables in PowerShell (Windows):

```powershell
# Required for both local and Docker environments
$env:GOOGLE_CLIENT_ID = "your_google_client_id"
$env:GOOGLE_CLIENT_SECRET = "your_google_client_secret"

# Required only for Docker environment
$env:CERTIFICATE_PASSWORD = "your_certificate_password"
$env:CERTIFICATE_PATH = "/certs/api_cert.pfx"
```

### Running the Application

#### Using Docker Compose

1. Build and start the containers:

```bash
docker-compose up --build
```

2. The API will be available at `https://localhost:7263`

#### Running Locally

1. Restore dependencies:

```bash
dotnet restore
```

2. Run the application:

```bash
dotnet run --project Products.API
```

3. The API will be available at `https://localhost:7263`

### Environment Variables

The following environment variables are required to run the application:

| Variable | Description | Required | Environment |
|----------|-------------|----------|-------------|
| `GOOGLE_CLIENT_ID` | Google OAuth Client ID for authentication | Yes | Both |
| `GOOGLE_CLIENT_SECRET` | Google OAuth Client Secret for authentication | Yes | Both |
| `CERTIFICATE_PASSWORD` | Password for the HTTPS certificate | Yes | Docker only |
| `CERTIFICATE_PATH` | Path to the HTTPS certificate in the container | Yes | Docker only |
| `ConnectionStrings__DefaultConnection` | SQLite database connection string | No (defaults to `/app/data/Products.db`) | Both |

### Docker Configuration

The application uses a multi-stage Docker build to optimize the image size. The Dockerfile includes:

- Base image for runtime
- Build stage for compilation
- Publish stage for the final application
- Certificate configuration for HTTPS

The docker-compose.yml file includes:
- Port mapping (7263:7263)
- Environment variable configuration
- Volume mapping for persistent data
- Automatic restart policy

### Development Notes

- The application uses SQLite for data storage, with the database file stored in the `data` directory
- HTTPS is required for all API endpoints
- Google OAuth is used for authentication
- Admin users have additional privileges for write operations
- Swagger UI is available at `https://localhost:7263/swagger` when running locally

### Troubleshooting

If you encounter issues with environment variables in Docker:

1. Check that environment variables are set in your shell
2. Verify the certificate exists in the `certs` directory
3. Check Docker logs for detailed error messages:
   ```bash
   docker-compose logs api
   ```
