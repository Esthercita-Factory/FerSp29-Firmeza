# Firmeza - ASP.NET Core Clean Architecture

Solución backend desarrollada con **.NET** bajo los principios de **Clean Architecture (Arquitectura Limpia)**, garantizando desacoplamiento, mantenibilidad y escalabilidad desde su concepción inicial[cite: 1].

---

## 🏛️ Arquitectura de la Solución

El proyecto organiza sus responsabilidades en capas bien definidas, asegurando que las reglas de negocio permanezcan aisladas de las dependencias externas y frameworks[cite: 1]:

```text
Firmeza/
├── src/
│   ├── Firmeza.Domain/           # Capa de Dominio: Entidades nucleares, enums e interfaces base
│   ├── Firmeza.Application/      # Capa de Aplicación: Casos de uso, DTOs y lógica de negocio
│   ├── Firmeza.Infrastructure/   # Capa de Infraestructura: Persistencia de datos y servicios externos
│   └── Firmeza.API/              # Capa de Presentación: Endpoints HTTP y configuración de OpenAPI
└── test/
    └── Firmeza.UnitTests/        # Pruebas automatizadas del sistema con xUnit
```

### Flujo de Dependencias

```text
[ Firmeza.API ] ────> [ Firmeza.Application ] <──── [ Firmeza.Infrastructure ]
       │                         │                               │
       │                         ▼                               │
       └──────────────────> [ Firmeza.Domain ] <─────────────────┘
```

* **Domain:** No depende de ningún otro proyecto.
* **Application:** Depende únicamente de `Domain`[cite: 1].
* **Infrastructure:** Implementa contratos definidos en `Application` e interactúa con `Domain`[cite: 1].
* **API:** Punto de entrada que orquesta la Inyección de Dependencias (DI) conectando `Application` e `Infrastructure`[cite: 1].
* **UnitTests:** Evalúa la lógica de `Application` y `Domain` en aislamiento total[cite: 1].

---

## ⚙️ Tecnologías Utilizadas

* **Lenguaje:** C#
* **Plataforma:** .NET (ASP.NET Core Minimal APIs / OpenAPI)
* **Framework de Pruebas:** xUnit[cite: 1]
* **IDE Recomendado:** JetBrains Rider / Visual Studio Code

---

## 🚀 Requisitos Previos

* [.NET SDK](https://dotnet.microsoft.com/download) instalado en su versión 8.0 o superior.
* Git instalado y configurado.

---

## 🛠️ Instalación y Configuración

1. **Clonar el repositorio:**
   ```bash
   git clone [https://github.com/Esthercita-Factory/FerSp29-Firmeza.git](https://github.com/Esthercita-Factory/FerSp29-Firmeza.git)
   cd FerSp29-Firmeza
   ```

2. **Restaurar dependencias:**
   ```bash
   dotnet restore
   ```

3. **Compilar la solución:**
   ```bash
   dotnet build
   ```

---

## 🧪 Ejecución de Pruebas Unitarias

Para correr la suite de pruebas automatizadas y validar la integridad del sistema:

```bash
dotnet test
```

---

## 🌐 Ejecutar la API Localmente

Para iniciar el servidor de desarrollo:

```bash
dotnet run --project src/Firmeza.API/Firmeza.API.csproj
```

Una vez en ejecución, accede a la documentación interactiva de la API navegando a la URL indicada en la terminal (OpenAPI / Swagger en entorno local de desarrollo).

---

## 🧩 Inyección de Dependencias (DI)

La solución utiliza métodos de extensión modulares para registrar las dependencias de cada capa sin acoplar la configuración en `Program.cs`[cite: 1]:

* `services.AddApplication()` registra casos de uso y validaciones[cite: 1].
* `services.AddInfrastructure()` registra la persistencia de datos y repositorios[cite: 1].
