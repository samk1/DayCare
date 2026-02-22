# DayCare

A .NET 7 ASP.NET Core MVC web application deployed to **Azure Container Apps**, with a fully automated Infrastructure as Code (IaC) pipeline powered by **Terraform CDK (CDKTF) in C#**.

---

## 🏗️ Infrastructure as Code Overview

All cloud infrastructure is defined and deployed as code. There are two layers of IaC in this repo:

| Layer | Technology | Location |
|---|---|---|
| Cloud infrastructure (current) | **CDKTF (C#)** + Terraform Cloud | `DayCare.Azure/` |
| Cloud infrastructure (original) | **Azure Bicep** | `DayCare.Web/main.bicep` |
| Container orchestration | **Docker Compose** | `docker-compose.yml` |
| CI/CD pipeline | **GitHub Actions** | `.github/workflows/` |

---

## ✨ Highlights

### 1. Terraform CDK (CDKTF) Written in C#

Rather than writing Terraform in HCL, this project uses **HashiCorp's Cloud Development Kit for Terraform (CDKTF)** with C# as the language. This means:

- Infrastructure is defined using a strongly-typed, object-oriented language with full IDE support, refactoring tools, and compiler checks.
- Shared abstractions are modelled as reusable C# classes (e.g. `ResourceGroup`, `SqlServer`, `ContainerRegistry`), avoiding repetition across stacks.
- The **`FindOrCreate` pattern** allows data sources to be declared once and safely reused across multiple stacks within the same Terraform workspace without creating duplicate constructs.

**CDKTF entry point:** [`DayCare.Azure/Program.cs`](DayCare.Azure/Program.cs)

```csharp
var app = new App();
new InfrastuctureStack(scope: app, containerAppName: "daycare-web");
new MigrationStack(scope: app, containerImage: ..., containerAppName: "daycare-web");
new ApplicationStack(scope: app, containerImage: ..., containerAppName: "daycare-web");
app.Synth();
```

---

### 2. Three Ordered Deployment Stacks

Infrastructure is split into three independent Terraform stacks deployed sequentially by CI/CD:

#### `InfrastructureStack`
Creates foundational Azure resources:
- **Azure SQL Server** (v12.0) with **Azure AD-only authentication** — no SQL username/password auth permitted.
- **Azure SQL Database** with `SQL_Latin1_General_CP1_CI_AS` collation.
- **SQL Server firewall rule** to allow Azure services.
- SQL Server admin is a **User Assigned Managed Identity**, added to the Azure AD **Directory Readers** group to enable Entra ID lookups.
- Database admin password sourced from **Azure Key Vault** — never hardcoded.

#### `MigrationStack`
Runs Entity Framework Core database migrations as a one-off container job:
- Deploys an **Azure Container Instance** (`RestartPolicy: Never`) that runs the EF migrations bundle baked into the Docker image.
- A **User Assigned Managed Identity** for the migration job is granted `db_owner` on the database via a **Azure Resource Deployment Script** (PowerShell), so migrations run without any database password.
- The container exits after migrations complete; the deployment script resource is cleaned up automatically (`CleanupPreference: Always`).

#### `ApplicationStack`
Deploys the web application:
- **Azure Container App** with `RevisionMode: Multiple` for zero-downtime blue/green deployments.
- A **User Assigned Managed Identity** for the app is granted `db_datareader` and `db_datawriter` roles via the same deployment-script mechanism as the migration stack.
- Ingress configured for HTTPS-only external traffic on port 80 with TLS termination handled by the Container Apps environment.
- Container image pulled from **Azure Container Registry** using admin credentials stored as a Container App secret.

---

### 3. Passwordless Database Connectivity (Managed Identity)

The application connects to Azure SQL using **Active Directory Default** authentication — no database password is stored or passed to the app at runtime:

```
Server=tcp:{server}.database.windows.net,1433;
Initial Catalog={db};
Encrypt=True;
Authentication="Active Directory Default";
```

Database role membership is provisioned at deploy time by a PowerShell script (`CreateDatabaseUser.ps1`) that runs inside an Azure Resource Deployment Script with an identity that has the necessary SQL Server permissions.

---

### 4. Secrets Management with Azure Key Vault

Secrets (e.g. the SQL Server admin password used during initial provisioning) are never hardcoded. They are read at deploy time from **Azure Key Vault** via CDKTF data sources:

```csharp
keyVaultSecrets.GetSecret("database-password")
```

Terraform Cloud is used as the remote backend, so Terraform state (which can contain sensitive output values) is never stored locally.

---

### 5. CI/CD Pipeline (GitHub Actions)

**[`.github/workflows/daycare20231105104246.yml`](.github/workflows/daycare20231105104246.yml)**

The pipeline has two jobs that run on every push to `master`:

```
push to master
    │
    ├─ buildImage
    │      Build multi-stage Docker image
    │      Tag with Git SHA + "latest"
    │      Push to Azure Container Registry
    │
    └─ terraform  (needs: buildImage)
           dotnet build DayCare.Azure
           cdktf deploy infrastructureStack   ← DB + networking
           cdktf deploy migrationsStack       ← EF Core schema migrations
           cdktf deploy applicationStack      ← Container App revision
```

Key points:
- **Immutable image tags** — each deployment uses the Git commit SHA as the container image tag, ensuring full traceability from running container back to source code.
- **Ordered stack deployment** — infrastructure is always converged before migrations run, and migrations always complete before the new application revision is live.
- **Terraform Cloud** (`TF_TOKEN_app_terraform_io`) manages remote state and provides an audit log of all infrastructure changes.

---

### 6. Multi-Stage Dockerfile with Bundled Migrations

**[`DayCare.Web/Dockerfile`](DayCare.Web/Dockerfile)**

The Dockerfile uses a **three-stage build**:

1. **`base`** — lightweight ASP.NET 7 runtime image.
2. **`build`** — SDK image; restores NuGet packages, compiles the application, and installs the `dotnet-ef` global tool.
3. **`publish`** — publishes the app *and* bundles the EF Core migrations into a self-contained binary (`/app/publish/Migrations`).
4. **`final`** — copies only the published output into the slim runtime image.

The bundled migrations binary means the same Docker image is used for both running the web app (`ENTRYPOINT`) and running the one-off migration job (`Commands: ["/app/Migrations"]`), keeping the two perfectly in sync.

---

### 7. Local Development with Docker Compose

**[`docker-compose.yml`](docker-compose.yml)**

Developers can run the full stack locally with a single command. Docker Compose spins up:
- The `daycare.web` container (built from the repo's Dockerfile).
- A **Microsoft SQL Server 2022** container on port 1433, so no local SQL Server installation is needed.

---

### 8. Azure Bicep (Historical Reference)

**[`DayCare.Web/main.bicep`](DayCare.Web/main.bicep)**

An earlier version of the infrastructure was defined in **Azure Bicep** (Microsoft's ARM template DSL). It provisions equivalent resources: SQL Server, SQL Database, Container App Environment, and Container App with role assignments. This file serves as a useful point of comparison showing the evolution from ARM-based tooling to CDKTF.

---

## 📁 Project Structure

```
DayCare/
├── .github/workflows/          # GitHub Actions CI/CD pipeline
├── DayCare.Azure/              # CDKTF (C#) infrastructure project
│   ├── Program.cs              # Entry point — instantiates all stacks
│   ├── cdktf.json              # CDKTF config + Azure context variables
│   ├── Scripts/
│   │   └── CreateDatabaseUser.ps1  # PowerShell run inside Azure Deployment Script
│   └── Stacks/
│       ├── BaseAzureStack.cs       # Shared Terraform Cloud backend + providers
│       ├── InfrastuctureStack.cs   # SQL Server, SQL Database, firewall
│       ├── MigrationStack.cs       # Container Instance for EF migrations
│       ├── ApplicationStack.cs     # Azure Container App
│       ├── Data/                   # Data sources (ResourceGroup, SqlServer, etc.)
│       └── Model/                  # Shared constructs (DatabaseAccess, Container)
├── DayCare.Web/                # ASP.NET Core 7 MVC web application
│   ├── Dockerfile              # Multi-stage build with bundled migrations
│   ├── main.bicep              # Original Bicep IaC (historical reference)
│   └── main.bicepparam         # Bicep parameter file
├── docker-compose.yml          # Local dev: web app + SQL Server
└── DayCare.sln
```

---

## 🔧 Tech Stack

| Concern | Technology |
|---|---|
| Web framework | ASP.NET Core 7 MVC |
| Database | Azure SQL (MSSQL) |
| ORM / Migrations | Entity Framework Core |
| Containerisation | Docker (multi-stage) |
| Container platform | Azure Container Apps |
| Container registry | Azure Container Registry |
| IaC tooling | CDKTF (C#) |
| IaC state backend | Terraform Cloud (HCP Terraform) |
| Secrets | Azure Key Vault |
| Identity | Azure Managed Identity (System & User Assigned) |
| CI/CD | GitHub Actions |
| Local dev | Docker Compose |
