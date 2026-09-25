# Firmeza - ASP.NET Core Clean Architecture & PostgreSQL

Solución backend desarrollada con **.NET 10** bajo los principios de **Clean Architecture (Arquitectura Limpia)** y enfoque **Database-First** con **PostgreSQL** y **Entity Framework Core**.

---

## 🏛️ Arquitectura de la Solución

El proyecto organiza sus responsabilidades en capas bien definidas, asegurando que las reglas de negocio permanezcan aisladas de las dependencias externas y frameworks:

```text
Firmeza/
├── src/
│   ├── Firmeza.Domain/           # Capa de Dominio: Entidades nucleares (UUIDs), enums e interfaces base
│   ├── Firmeza.Application/      # Capa de Aplicación: Casos de uso, DTOs y lógica de negocio
│   ├── Firmeza.Infrastructure/   # Capa de Infraestructura: Persistencia de datos (EF Core, PostgreSQL)
│   └── Firmeza.API/              # Capa de Presentación: Endpoints HTTP, OpenAPI y Scalar UI
└── test/
    └── Firmeza.UnitTests/        # Pruebas unitarias y de integración con xUnit
```

### Flujo de Dependencias

```text
[ Firmeza.API ] ────> [ Firmeza.Application ] <──── [ Firmeza.Infrastructure ]
       │                         │                               │
       │                         ▼                               │
       └──────────────────> [ Firmeza.Domain ] <─────────────────┘
```

* **Domain:** No depende de ningún otro proyecto. Contiene las entidades (`Cliente`, `Producto`, `Venta`, `Detalle`).
* **Application:** Depende únicamente de `Domain`.
* **Infrastructure:** Implementa contratos definidos en `Application`, interactúa con `Domain` y gestiona la persistencia con `ApplicationDbContext`.
* **API:** Punto de entrada que orquesta la Inyección de Dependencias (DI) conectando `Application` e `Infrastructure`.
* **UnitTests:** Evalúa la arquitectura, el modelo de datos relacional y pruebas de integración.

---

## 🗄️ Base de Datos Relacional (PostgreSQL)

El entorno de base de datos se ejecuta mediante Docker Compose.

### Iniciar el contenedor de PostgreSQL
```bash
docker compose up -d
```

### Configuración de Conexión
* **Host:** `localhost`
* **Puerto:** `5433` (mapeado al 5432 del contenedor)
* **Base de datos:** `firmeza_db`
* **Usuario:** `postgres`
* **Contraseña:** `postgres`
* **Cadena de conexión:**
  ```
  Host=localhost;Port=5433;Database=firmeza_db;Username=postgres;Password=postgres
  ```

### Tablas del Esquema (Identificadores UUID / GUID)
1. **`clientes`**: Registro de clientes (`id UUID`, `nombre`, `correo`, `telefono`, `direccion`, `fecha_registro`).
2. **`productos`**: Catálogo de productos (`id UUID`, `codigo`, `nombre`, `descripcion`, `precio`, `stock`, `activo`, `fecha_creacion`).
3. **`ventas`**: Registro de ventas/facturas (`id UUID`, `cliente_id UUID`, `fecha`, `total`, `estado`).
4. **`detalles`**: Líneas de venta con relación a venta y producto (`id UUID`, `venta_id UUID`, `producto_id UUID`, `cantidad`, `precio_unitario`, `subtotal`).

El script de inicialización con constraints, índices y seed data se encuentra en `scripts/init.sql`.

---

## ⚙️ Entity Framework Core (Database-First)

El modelado y persistencia se generó mediante ingeniería inversa (`Scaffold-DbContext` / `dotnet ef dbcontext scaffold`):

```bash
dotnet ef dbcontext scaffold "Host=localhost;Port=5433;Database=firmeza_db;Username=postgres;Password=postgres" \
  Npgsql.EntityFrameworkCore.PostgreSQL \
  --project src/Firmeza.Infrastructure \
  --startup-project src/Firmeza.API \
  --context ApplicationDbContext \
  --context-dir Persistence \
  --output-dir ../Firmeza.Domain/Entities \
  --namespace Firmeza.Domain.Entities \
  --context-namespace Firmeza.Infrastructure.Persistence \
  --no-onconfiguring \
  --force
```

---

## 🔄 Migraciones de Entity Framework Core

El proyecto cuenta con migraciones versionadas en [`src/Firmeza.Infrastructure/Persistence/Migrations`](src/Firmeza.Infrastructure/Persistence/Migrations).

### Crear una nueva migración
```bash
dotnet ef migrations add <NombreMigracion> --project src/Firmeza.Infrastructure --startup-project src/Firmeza.API --output-dir Persistence/Migrations
```

### Aplicar migraciones pendientes a la base de datos
```bash
dotnet ef database update --project src/Firmeza.Infrastructure --startup-project src/Firmeza.API
```

---

## 🖥️ Conexión desde pgAdmin 4

Para administrar visualmente la base de datos desde pgAdmin 4:

1. Abre **pgAdmin 4**.
2. Clic derecho en **Servers** -> **Register** -> **Server...**.
3. En la pestaña **General**:
   * **Name:** `Firmeza PostgreSQL`
4. En la pestaña **Connection**:
   * **Host name/address:** `localhost` (o `127.0.0.1`)
   * **Port:** `5433` *(Importante: no usar 5432)*
   * **Maintenance database:** `firmeza_db`
   * **Username:** `postgres`
   * **Password:** `postgres`
   * *(Opcional)* Marca la casilla **Save password**.
5. Clic en **Save**.

En el árbol lateral podrás explorar las tablas en:
`Servers` > `Firmeza PostgreSQL` > `Databases` > `firmeza_db` > `Schemas` > `public` > `Tables`.

---

## 🧪 Ejecución de Pruebas Unitarias e Integración

Para correr la suite completa de pruebas:

```bash
dotnet test
```

---

## 🌐 Ejecutar la API Localmente

Para iniciar el servidor de desarrollo:

```bash
dotnet run --project src/Firmeza.API/Firmeza.API.csproj
```

Una vez en ejecución, accede a la documentación interactiva navegando a [http://localhost:5117/](http://localhost:5117/) (redirecciona a Scalar API Reference).
