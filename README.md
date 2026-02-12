# SAPFIAI Clean Architecture Solution Template

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/download)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

## 📋 Descripción

SAPFIAI es una plantilla de solución empresarial basada en **Clean Architecture** y **ASP.NET Core 8.0**. El objetivo de esta plantilla es proporcionar un enfoque directo y eficiente para el desarrollo de aplicaciones empresariales, aprovechando el poder de la Arquitectura Limpia y las capacidades modernas de .NET 8.

Esta plantilla permite crear aplicaciones Web API robustas, escalables y mantenibles, siguiendo los principios SOLID y las mejores prácticas de la industria.

---

## ✨ Características Principales

- **Clean Architecture**: Separación clara de responsabilidades en capas (Domain, Application, Infrastructure, Web)
- **ASP.NET Core 8.0**: Framework moderno y de alto rendimiento
- **Entity Framework Core 8**: ORM potente con soporte para migraciones y múltiples bases de datos
- **CQRS Pattern**: Implementado mediante MediatR para separar comandos y consultas
- **Validación Automática**: FluentValidation integrado en el pipeline de MediatR
- **Mapeo de Objetos**: AutoMapper para transformaciones DTO ↔ Entidades
- **Testing Completo**: NUnit, FluentAssertions, Moq y Respawn para pruebas unitarias e integración
- **CI/CD Ready**: Pipeline completo de integración y despliegue continuo

---

## 🚀 Requisitos Previos

Para construir y ejecutar la solución, necesitas:

- **.NET 8.0 SDK** (última versión) - [Descargar aquí](https://dotnet.microsoft.com/download/dotnet/8.0)
- **SQL Server** o **SQLite** (según configuración)
- **Visual Studio 2022** o **Visual Studio Code** (recomendado)
- **Git** para control de versiones

---

## 📦 Instalación

### 1. Instalar la Plantilla .NET

```bash
dotnet new install Clean.Architecture.Solution.Template::8.0.6
```

### 2. Crear un Nuevo Proyecto

Para crear una solución **Web API únicamente** (sin frontend):

```bash
dotnet new ca-sln -cf None -o SAPFIAI
```

Para crear con frontend Angular:

```bash
dotnet new ca-sln --client-framework Angular --output SAPFIAI
```

Para crear con frontend React:

```bash
dotnet new ca-sln -cf React -o SAPFIAI
```

### 3. Navegar al Proyecto

```bash
cd SAPFIAI/src/Web
```

### 4. Ejecutar la Aplicación

```bash
dotnet run
```

La API estará disponible en:
- **HTTPS**: `https://localhost:5001`
- **HTTP**: `http://localhost:5000`
- **Swagger UI**: `https://localhost:5001/swagger`

---

## 🏗️ Estructura del Proyecto

```
SAPFIAI/
├── src/
│   ├── Domain/              # Entidades, Enums, Excepciones, Interfaces
│   ├── Application/         # Lógica de negocio, DTOs, CQRS, Validaciones
│   ├── Infrastructure/      # Implementación de EF Core, Servicios externos
│   └── Web/                # API Controllers, Middleware, Configuración
├── tests/
│   ├── Application.UnitTests/
│   ├── Application.IntegrationTests/
│   └── Domain.UnitTests/
└── docs/                   # Documentación adicional
```

### Descripción de Capas

#### **Domain** (Núcleo)
- Contiene las entidades del dominio
- Reglas de negocio fundamentales
- Interfaces de repositorios
- **Sin dependencias** de otras capas

#### **Application** (Casos de Uso)
- Lógica de aplicación (Commands & Queries)
- DTOs y mapeos
- Validaciones con FluentValidation
- Interfaces de servicios
- Depende únicamente de **Domain**

#### **Infrastructure** (Detalles de Implementación)
- Implementación de Entity Framework Core
- Repositorios concretos
- Servicios de infraestructura (Email, Storage, etc.)
- Migraciones de base de datos
- Depende de **Application**

#### **Web** (Presentación)
- API Controllers
- Configuración de dependencias
- Middleware personalizado
- Manejo de excepciones global
- Punto de entrada de la aplicación

---

## 💾 Base de Datos

### Configuración por Defecto

La plantilla está configurada para usar **SQL Server** por defecto. La cadena de conexión se encuentra en `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=SAPFIAIDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

### Usar SQLite (Alternativa)

Si prefieres usar SQLite, crea la solución con:

```bash
dotnet new ca-sln --use-sqlite -cf None -o SAPFIAI
```

### Migraciones

La base de datos se crea automáticamente al ejecutar la aplicación por primera vez. Las migraciones se aplican automáticamente.

#### Crear una Nueva Migración

Desde la raíz del proyecto:

```bash
dotnet ef migrations add "MigracionEjemplo" --project src/Infrastructure --startup-project src/Web --output-dir Data/Migrations
```

#### Aplicar Migraciones Manualmente

```bash
dotnet ef database update --project src/Infrastructure --startup-project src/Web
```

#### Revertir Migración

```bash
dotnet ef database update MigracionAnterior --project src/Infrastructure --startup-project src/Web
```

---

## 🔧 Crear Casos de Uso (Commands & Queries)

### Crear un Comando

Navega a `./src/Application` y ejecuta:

```bash
dotnet new ca-usecase --name CreateTodoList --feature-name TodoLists --usecase-type command --return-type int
```

Esto generará:
- `CreateTodoListCommand.cs` - Comando
- `CreateTodoListCommandHandler.cs` - Manejador
- `CreateTodoListCommandValidator.cs` - Validador

### Crear una Query

```bash
dotnet new ca-usecase -n GetTodos -fn TodoLists -ut query -rt TodosVm
```

### Ver Opciones Disponibles

```bash
dotnet new ca-usecase --help
```

---

## 🌐 Despliegue en SmarterASP.NET

### Prerequisitos

1. Cuenta activa en [SmarterASP.NET](https://www.smarterasp.net/)
2. Plan compatible con **.NET 8.0**
3. Acceso a SQL Server (incluido en planes compatibles)

### Pasos de Despliegue

#### 1. Configuración Local

**Actualizar `appsettings.Production.json`:**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=TU_SERVIDOR_SMARTERASP;Database=TU_BASE_DATOS;User Id=TU_USUARIO;Password=TU_PASSWORD;TrustServerCertificate=true"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

#### 2. Publicar la Aplicación

Desde `src/Web`:

```bash
dotnet publish -c Release -o ./publish
```

#### 3. Configurar Base de Datos en SmarterASP

1. Accede al **Panel de Control** de SmarterASP
2. Ve a **SQL Server** → **Create Database**
3. Anota:
   - Servidor
   - Nombre de base de datos
   - Usuario
   - Contraseña

#### 4. Aplicar Migraciones

**Opción A: Script SQL**

Genera el script SQL:

```bash
dotnet ef migrations script --project src/Infrastructure --startup-project src/Web --output migration.sql
```

Ejecuta el script en el **SQL Server Manager** de SmarterASP.

**Opción B: Desde Publicación**

Configura en `src/Web/Web.csproj`:

```xml
<PropertyGroup>
  <PublishDatabaseSettings>true</PublishDatabaseSettings>
</PropertyGroup>
```

#### 5. Subir Archivos por FTP

1. **Obtén credenciales FTP** desde el panel de SmarterASP
2. **Conecta con FileZilla** o cliente FTP preferido
3. **Sube el contenido** de `./publish` a la carpeta raíz del sitio (usualmente `wwwroot`)

#### 6. Configurar IIS en SmarterASP

1. Ve a **IIS Settings** en el panel
2. Configura:
   - **Application Pool**: .NET CLR Version → No Managed Code
   - **Pipeline Mode**: Integrated
   - **.NET Framework Version**: .NET 8.0

#### 7. Configurar web.config

Asegúrate de que `web.config` exista en la raíz (se genera automáticamente al publicar):

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <system.webServer>
    <handlers>
      <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
    </handlers>
    <aspNetCore processPath="dotnet" 
                arguments=".\Web.dll" 
                stdoutLogEnabled="false" 
                stdoutLogFile=".\logs\stdout" 
                hostingModel="inprocess" />
  </system.webServer>
</configuration>
```

#### 8. Verificar Despliegue

Accede a:
- `https://tudominio.com/swagger` - Documentación API
- `https://tudominio.com/health` - Health check (si está configurado)

---

## 📊 Pipeline CI/CD

La plantilla incluye un pipeline completo para **Azure DevOps** o **GitHub Actions**.

### Configuración GitHub Actions

Crea `.github/workflows/deploy.yml`:

```yaml
name: Deploy to SmarterASP

on:
  push:
    branches: [ main ]

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 8.0.x
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build --configuration Release --no-restore
    
    - name: Test
      run: dotnet test --no-build --verbosity normal
    
    - name: Publish
      run: dotnet publish src/Web/Web.csproj -c Release -o ./publish
    
    - name: Deploy to FTP
      uses: SamKirkland/FTP-Deploy-Action@4.3.0
      with:
        server: ${{ secrets.FTP_SERVER }}
        username: ${{ secrets.FTP_USERNAME }}
        password: ${{ secrets.FTP_PASSWORD }}
        local-dir: ./publish/
```

---

## 🧪 Testing

### Ejecutar Todas las Pruebas

```bash
dotnet test
```

### Ejecutar Pruebas con Coverage

```bash
dotnet test /p:CollectCoverage=true /p:CoverageReportFormat=opencover
```

### Tipos de Pruebas Incluidas

- **Unit Tests**: Pruebas de lógica de negocio aislada
- **Integration Tests**: Pruebas end-to-end con base de datos en memoria
- **Functional Tests**: Pruebas de API completas

---

## 🛠️ Tecnologías Utilizadas

| Tecnología | Versión | Propósito |
|------------|---------|-----------|
| ASP.NET Core | 8.0 | Framework web |
| Entity Framework Core | 8.0 | ORM |
| MediatR | 12.x | Patrón mediador (CQRS) |
| AutoMapper | 12.x | Mapeo objeto-objeto |
| FluentValidation | 11.x | Validaciones |
| NUnit | 3.x | Framework de pruebas |
| FluentAssertions | 6.x | Aserciones legibles |
| Moq | 4.x | Mocking |
| Respawn | 6.x | Limpieza de base de datos en tests |

---

## 📚 Recursos y Documentación

- [Clean Architecture con ASP.NET Core (GOTO 2019)](https://www.youtube.com/watch?v=dK4Yb6-LxAk)
- [Documentación de .NET 8](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-8)
- [Entity Framework Core 8](https://learn.microsoft.com/en-us/ef/core/)



