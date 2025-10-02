
TastyTable Backend

TastyTable is a backend project developed by four students:

Mustafa Elmi Muse

Liibaan Olow

Ahmed Ismail Musse

Mahamed Hassan Ali

About the Project

TastyTable is a monolithic backend solution for managing restaurant menus, user authentication, customer orders, and table reservations. Developed as part of a backend programming course, the project focuses on modern and relevant technologies to create a secure, maintainable, and extendable system.

The backend follows a clean architecture with separation of concerns across layers:

API Layer: Exposes endpoints, handles authentication, and integrates Swagger

Core Layer: Defines entities, DTOs, and interfaces

Services Layer: Implements business logic for users, menus, and orders

Data Layer: Integrates MySQL with EF Core, manages migrations and seeding

Tests Layer: Unit tests with xUnit and Moq

Key Features

User registration and login with JWT tokens

Admin management of menu items and availability

Users can browse menus, place orders, and make table reservations

Demo data seeding and database setup included

Fully tested business logic

Technologies and Tools

Backend Framework: ASP.NET Core (.NET, C#)

Database: MySQL

Authentication: JWT (JSON Web Tokens)

Version Control: Git and GitHub

API Documentation: Swagger / OpenAPI

TastyTable demonstrates secure backend development, role-based access control, unit testing, and deployment readiness, making it suitable for integration with web or mobile applications.

Installation

Clone the repository:

git clone https://github.com/mustafa3m/TastyTable-final-1.git
cd TastyTable


Install dependencies:
Open the solution in Visual Studio or use CLI:

dotnet restore


Set up the database:

Create a MySQL database

Update the connection string in appsettings.json

Run migrations (if used):

dotnet ef database update


Start the server:

dotnet run

# TastyTable (.NET 8 + MySQL + JWT + Docker + AWS)

Monolithic Web API built with **ASP.NET Core 8**, **Entity Framework Core (MySQL)**, and **JWT authentication**.  
Architecture: **Controllers → Services → Repositories**.  
Runs on **Linux, Windows, Mac** with Docker or directly with .NET SDK.  
Supports deployment to **AWS ECS (Fargate)** or **AWS App Runner** with **RDS MySQL**.

---

## 🚀 Prerequisites

### Windows / Linux / Mac

- **.NET 8 SDK**
  - [Download](https://dotnet.microsoft.com/download/dotnet/8.0) or use your package manager  
  - Example (Ubuntu):  
    ```bash
    sudo snap install dotnet-sdk --classic --channel=8.0
    sudo snap alias dotnet-sdk.dotnet dotnet
    ```

- **EF Core tools**
  ```bash
  dotnet tool install --global dotnet-ef
  ```

- **Docker + Compose**
  - [Install Docker Desktop](https://www.docker.com/products/docker-desktop/) (Windows/Mac)  
  - Linux:  
    ```bash
    sudo apt update && sudo apt install -y docker.io docker-compose-plugin
    sudo usermod -aG docker $USER
    ```
    Logout/login to apply group changes.

---

## 🐳 Run with Docker Compose (local)

```bash
docker compose up --build
```

- API → http://localhost:8080/swagger  
- MySQL → port `3306` (inside container)

### Services

- **db** → MySQL 8, seeded with `TastyTableDb`  
  user: `root` / password: `Mohamed2009`
- **api** → TastyTable .NET 8 Web API

---

## 🔑 Seeded Data

- Admin user: `admin` / `Admin@123`
- Menu items: Margherita Pizza, Chicken Biryani, Beef Burger

---

## 🔐 Auth Flow

1. **Register** → `POST /api/auth/register`  
   Swagger form inputs: username, email, password

2. **Login** → `POST /api/auth/login`  
   Swagger form inputs: username, password  
   Copy token.

3. **Authorize** in Swagger (🔒 button) → paste:  
   ```http
   Bearer <token>
   ```

---

## 🍽️ Menu Endpoints

- `GET /api/menu` → list menu items
- `POST /api/menu` (Admin only) → create item  
- `PATCH /api/menu/{id}/availability?isAvailable=false` (Admin only)
- `DELETE /api/menu/{id}` (Admin only)

---

## 🛒 Orders Endpoints

- `POST /api/orders` → Create a new order  
  ```json
  { "items": [ { "menuItemId": 1, "quantity": 2 } ] }
  ```

- `GET /api/orders` → List logged-in user orders  
- `GET /api/orders/{id}` → Get order details  

- `PUT /api/orders/{id}` → Update an order’s items  
  ```json
  { "items": [ { "menuItemId": 2, "quantity": 3 } ] }
  ```

- `PATCH /api/orders/{id}/status` (Admin only) → Update order status  
  ```json
  { "status": "Completed" }
  ```

- `DELETE /api/orders/{id}` (Admin only) → Delete an order  

---

## 🧪 Run Tests

Unit tests use **xUnit** and **Moq**.

```bash
dotnet test
```

---

## ☁️ AWS Deployment (ECS Fargate + RDS)

### Quick Summary
1. Build & test locally with Compose.  
2. Push image to ECR.  
3. Create RDS MySQL 8 instance.  
4. Create ECS Fargate service or App Runner service.  
   - Env vars:  
     - `ConnectionStrings__DefaultConnection=server=<RDS_ENDPOINT>;port=3306;database=TastyTableDb;user=<DB_USER>;password=<DB_PASS>;`  
     - `ASPNETCORE_ENVIRONMENT=Production`
   - Port mapping: container 8080 → LB port 80/443  
5. Run EF migrations (auto on startup, or manually via one-off task).

---

## ⚠️ Notes

- Replace `Jwt:Secret` in `appsettings.json` for production.  
- Secure DB credentials with **AWS Secrets Manager** or **Parameter Store**.  
- For HTTPS locally, run behind Nginx/Traefik or use Docker Desktop TLS.  
- Gateway is optional (reverse proxy).  
