# Deploy TastyTable to AWS Elastic Beanstalk (API) + Amazon RDS (MySQL)

This guide deploys the **TastyTable** ASP.NET Core 8 API to **AWS Elastic Beanstalk** (Docker on Amazon Linux 2) and uses **Amazon RDS (MySQL 8)** as the database.

Your repository already contains:
- `TastyTable.Api/Dockerfile` (multi-stage .NET 8 build)
- `.dockerignore`

The app runs EF Core migrations on startup (`ctx.Database.Migrate()`), so the database schema will be created/updated automatically.

---

## 0) Prerequisites

- AWS account with permissions for EB, EC2, RDS, IAM, VPC
- **AWS CLI** and **EB CLI** installed & configured (`aws configure`, `eb --version`)
- A domain & ACM certificate (optional, for HTTPS)

---

## 1) Create RDS (MySQL 8)

1. In **Amazon RDS** → **Create database**
   - Engine: **MySQL 8.x**
   - DB name: `TastyTableDb`
   - User: `admin` (or your choice)
   - Password: strong password
   - VPC: same one as EB
   - Public access: **No**
   - Security group: create `rds-tastytable-sg`
2. Note the **RDS endpoint** (e.g., `tastytable.xxxxx.ap-south-1.rds.amazonaws.com`).

Add **inbound rule** to the RDS SG:
- Type: MySQL/Aurora (3306)
- Source: EB EC2 security group (after EB is created)

---

## 2) Prepare EB source bundle

Elastic Beanstalk builds from your `Dockerfile`.

Project layout:
```
TastyTable/
├── TastyTable.sln
├── .dockerignore
├── TastyTable.Api/
│   ├── Dockerfile
│   └── ...
├── TastyTable.Core/
├── TastyTable.Data/
├── TastyTable.Services/
└── TastyTable.Tests/
```

Zip the **contents** of the root folder (not the folder itself):

```bash
cd TastyTable
zip -r ../tastytable-eb-source.zip . -x "**/bin/*" "**/obj/*" ".git/*"
```

---

## 3) Create Elastic Beanstalk environment

### Console
1. Elastic Beanstalk → Applications → Create application
2. App name: `tastytable`
3. Platform: **Docker** (Amazon Linux 2)
4. Upload `tastytable-eb-source.zip`
5. Configure environment:
   - Type: Load balanced
   - Instance type: `t3.micrgitt` or bigger
   - VPC: same as RDS
   - Subnets: at least 2 public + 2 private
   - Security groups: EB creates one automatically
6. Environment variables:
   - `ASPNETCORE_ENVIRONMENT=Production`
   - `ConnectionStrings__DefaultConnection=server=<RDS_ENDPOINT>;port=3306;database=TastyTableDb;user=<DB_USER>;password=<DB_PASS>;`

### EB CLI
```bash
eb init --platform "Docker running on 64bit Amazon Linux 2" --region ap-south-1
eb create tastytable-env --single --instance_type t3.micro   --envvars ASPNETCORE_ENVIRONMENT=Production,ConnectionStrings__DefaultConnection="server=<RDS_ENDPOINT>;port=3306;database=TastyTableDb;user=<DB_USER>;password=<DB_PASS>;"
eb deploy tastytable-env
eb open tastytable-env
```

---

## 4) Port and health check

- Dockerfile exposes **8080**
- EB maps Load Balancer 80/443 → container 8080
- Health check path: `/` or `/swagger`

---

## 5) Database migrations

Migrations run on startup automatically:

```csharp
using (var scope = app.Services.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    ctx.Database.Migrate();
    DemoDataSeeder.Seed(ctx);
}
```

If you prefer manual:
```bash
dotnet ef database update --project TastyTable.Data --startup-project TastyTable.Api
```

---

## 6) HTTPS

1. Request ACM certificate
2. EB Config → Load balancer → add HTTPS listener (443) with certificate
3. Redirect HTTP→HTTPS at LB or app middleware (optional)

---

## 7) Logs, scaling, and cleanup

- Logs: EB Console → Logs
- Auto-scaling: configure min/max instances
- Rolling updates: enable for zero-downtime deploys
- Cleanup: terminate EB env, delete RDS, SGs, ALB, EBS volumes

---

## Example connection string

```
ConnectionStrings__DefaultConnection=server=tastytable.abc123.ap-south-1.rds.amazonaws.com;port=3306;database=TastyTableDb;user=admin;password=StrongPassword!;
```

**Do not commit secrets**. Use EB env vars, AWS Secrets Manager, or Parameter Store.

---

🎉 You’re live!
- EB URL: `http://<env-name>.<random>.<region>.elasticbeanstalk.com`
- Or attach a Route53 domain + HTTPS
