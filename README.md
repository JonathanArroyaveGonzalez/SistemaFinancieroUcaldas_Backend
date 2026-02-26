# SAPFIAI - Plantilla Técnica Clean Architecture

[![.NET 8.0](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/download)

## Descripción Técnica

SAPFIAI es una solución empresarial basada en Clean Architecture y ASP.NET Core 8.0, orientada a la construcción de APIs robustas, escalables y mantenibles. El diseño desacopla la lógica de dominio, aplicación, infraestructura y presentación, facilitando pruebas, despliegue y evolución.

---

## Características Técnicas

- Arquitectura multicapa: Domain, Application, Infrastructure, Web
- ASP.NET Core 8.0 y Entity Framework Core 8
- CQRS con MediatR
- Validación centralizada con FluentValidation
- Mapeo DTO/Entidad con AutoMapper
- Testing: NUnit, FluentAssertions, Moq, Respawn
- Pipeline CI/CD listo para GitHub Actions y Azure DevOps

---

## Requisitos

- .NET 8.0 SDK ([descarga](https://dotnet.microsoft.com/download/dotnet/8.0))
- SQL Server o SQLite
- Visual Studio 2022+ o VS Code
- Git

---

## Instalación y Ejecución

1. Clona el repositorio y restaura dependencias:
   ```bash
   git clone <url>
   cd SAPFIAI
   dotnet restore
   ```
2. Configura la base de datos en `src/Web/appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=SAPFIAIDb;Trusted_Connection=true;MultipleActiveResultSets=true"
     }
   }
   ```
3. Ejecuta migraciones y la aplicación:
   ```bash
   dotnet ef database update --project src/Infrastructure --startup-project src/Web
   dotnet run --project src/Web
   ```
4. Accede a la API:
   - Swagger: https://localhost:5001/swagger
   - Health: https://localhost:5001/health

---

## Estructura de Carpetas

```
src/
  Domain/         # Entidades, lógica de dominio, interfaces
  Application/    # Casos de uso, CQRS, validaciones, DTOs
  Infrastructure/ # Persistencia, servicios externos, migraciones
  Web/            # API, configuración, middleware, endpoints
tests/
  Application.UnitTests/
  Application.FunctionalTests/
  Domain.UnitTests/
docs/             # Documentación técnica
```

---

## Flujo de Desarrollo

### Crear un Caso de Uso (CQRS)

1. Comando:
   ```bash
   dotnet new ca-usecase --name CrearUsuario --feature-name Usuarios --usecase-type command --return-type int
   ```
2. Query:
   ```bash
   dotnet new ca-usecase --name ObtenerUsuarios --feature-name Usuarios --usecase-type query --return-type List<UsuarioDto>
   ```
3. Validar opciones:
   ```bash
   dotnet new ca-usecase --help
   ```

---

## Migraciones y Base de Datos

- Crear migración:
  ```bash
  dotnet ef migrations add "NombreMigracion" --project src/Infrastructure --startup-project src/Web --output-dir Data/Migrations
  ```
- Aplicar migraciones:
  ```bash
  dotnet ef database update --project src/Infrastructure --startup-project src/Web
  ```
- Generar script SQL:
  ```bash
  dotnet ef migrations script --project src/Infrastructure --startup-project src/Web --output migration.sql
  ```

---

## Testing

- Ejecutar todos los tests:
  ```bash
  dotnet test
  ```
- Ejecutar con cobertura:
  ```bash
  dotnet test /p:CollectCoverage=true /p:CoverageReportFormat=opencover
  ```
- Tipos de pruebas:
  - Unitarias: lógica aislada
  - Funcionales: API end-to-end
  - Integración: persistencia y servicios

---

## Despliegue (SmarterASP.NET)

1. Configura `appsettings.Production.json` con la cadena de conexión de producción.
2. Publica la app:
   ```bash
   dotnet publish src/Web -c Release -o ./publish
   ```
3. Sube el contenido de `./publish` vía FTP a tu hosting.
4. Aplica migraciones en SQL Server (script o desde la app).
5. Verifica endpoints `/swagger` y `/health`.

---

## CI/CD con GitHub Actions

Ejemplo de workflow `.github/workflows/deploy.yml`:
```yaml
name: Deploy
on:
  push:
    branches: [ main ]
jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: 8.0.x
      - name: Restore
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

## Tecnologías Clave

| Tecnología              | Versión | Rol principal                |
|------------------------|---------|------------------------------|
| ASP.NET Core           | 8.0     | Framework web/API            |
| Entity Framework Core  | 8.0     | ORM y migraciones            |
| MediatR                | 12.x    | CQRS y mediador              |
| AutoMapper             | 12.x    | Mapeo DTO/Entidad            |
| FluentValidation       | 11.x    | Validación                   |
| NUnit                  | 3.x     | Testing unitario             |
| FluentAssertions       | 6.x     | Aserciones legibles          |
| Moq                    | 4.x     | Mocking                      |
| Respawn                | 6.x     | Reset de BD en tests         |

---

## Recursos

- [Clean Architecture con ASP.NET Core (GOTO 2019)](https://www.youtube.com/watch?v=dK4Yb6-LxAk)
- [Documentación oficial .NET 8](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-8)
- [Entity Framework Core 8](https://learn.microsoft.com/en-us/ef/core/)



