# RouteOps — ASP.NET Core API

Stack: .NET 9 · PostgreSQL · EF Core · MediatR · Hangfire · FCM

## Estructura del proyecto

```
RouteOps/
├── src/
│   ├── RouteOps.Domain/          # Entidades puras, enums, value objects
│   │   ├── Entities/             # Client, Order, Product, Credit, Sale...
│   │   └── Enums/                # OrderStatus, PayMethod, CreditStatus...
│   │
│   ├── RouteOps.Application/     # Lógica de negocio (CQRS)
│   │   ├── Commands/             # CreateOrder, ApproveOrder, ApplyPayment...
│   │   ├── Queries/              # GetDashboard, GetUpcomingDues, GetRoutes...
│   │   ├── Interfaces/           # IRouteOpsDbContext, INotificationService...
│   │   └── Validators/           # FluentValidation por cada command
│   │
│   ├── RouteOps.Infrastructure/  # Implementaciones concretas
│   │   ├── Persistence/          # DbContext + EF Configurations
│   │   └── Services/             # RouteOptimizer2Opt, CreditCheckJob, FCM...
│   │
│   └── RouteOps.API/             # Web API — punto de entrada
│       ├── Controllers/          # OrdersController, CreditsController...
│       └── Middleware/           # ExceptionHandlingMiddleware
│
└── tests/
    ├── RouteOps.UnitTests/
    └── RouteOps.IntegrationTests/
```

## Setup local

### 1. Requisitos
- .NET 9 SDK
- PostgreSQL 14+ (local o Docker)
- Visual Studio 2022 / Rider / VS Code

### 2. Base de datos local con Docker (opcional)
```bash
docker run --name routeops-pg \
  -e POSTGRES_PASSWORD=routeops123 \
  -e POSTGRES_DB=routeops_dev \
  -p 5432:5432 -d postgres:16
```

### 3. Configurar appsettings
Crea `src/RouteOps.API/appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=routeops_dev;Username=postgres;Password=routeops123"
  }
}
```

### 4. Aplicar migraciones
```bash
cd src/RouteOps.API
dotnet ef migrations add InitialCreate --project ../RouteOps.Infrastructure
dotnet ef database update
```
> Las migraciones también se aplican automáticamente al iniciar en Development.

### 5. Ejecutar
```bash
dotnet run --project src/RouteOps.API
```
- API: https://localhost:5001
- Swagger: https://localhost:5001/swagger
- Hangfire: https://localhost:5001/jobs

---

## Endpoints principales

### Pedidos
| Método | Ruta | Descripción |
|--------|------|-------------|
| POST | /api/orders | Crear pedido |
| PATCH | /api/orders/{id}/reception | Enviar a recepción |
| PATCH | /api/orders/{id}/approve | Aprobar (genera venta + crédito) |
| PATCH | /api/orders/{id}/reject | Rechazar |
| PATCH | /api/orders/{id}/delivered | Marcar entregado |

### Créditos
| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | /api/credits/upcoming-dues | Créditos por vencer |
| POST | /api/credits/{id}/payments | Registrar abono |

### Rutas
| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | /api/routes?optimize=true | Rutas por repartidor (con 2-opt) |

### Dashboard
| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | /api/dashboard | Métricas generales |

---

## Deploy en Railway

1. Conecta tu repo de GitHub en Railway
2. Agrega las variables de entorno:
   - `ConnectionStrings__DefaultConnection` → tu cadena de PostgreSQL
   - `ASPNETCORE_ENVIRONMENT` → `Production`
3. Railway detecta el proyecto .NET automáticamente y hace el build

## Deploy en Supabase (solo DB)

1. Crea un proyecto en supabase.com
2. Ve a SQL Editor y ejecuta `routeops_schema.sql`
3. Copia la connection string desde Settings → Database
4. Úsala en `DefaultConnection`

---

## Job nocturno (Hangfire)

`CreditCheckJob` corre todos los días a las 8 AM UTC y:
- Marca créditos vencidos con `status = 'overdue'`
- Envía WhatsApp a clientes con crédito que vence en 1, 3 o 7 días
- Registra cada notificación en la tabla `notifications`

Puedes dispararlo manualmente desde `/jobs` durante pruebas.
