# ?? Sistema de Autenticación y Seguridad SAPFIAI

## ?? Tabla de Contenidos

1. [Arquitectura General](#-arquitectura-general)
2. [Diagrama de Entidad-Relación](#-diagrama-de-entidad-relación)
3. [Endpoints de Autenticación](#-endpoints-de-autenticación)
4. [Endpoints de Seguridad (Admin)](#-endpoints-de-seguridad-admin)
5. [Flujo Completo de Login](#-flujo-completo-de-login)
6. [Flujo de Refresh Token](#-flujo-de-refresh-token)
7. [Protección Anti Fuerza Bruta](#-protección-anti-fuerza-bruta)
8. [Entidades de Base de Datos](#-entidades-de-base-de-datos)
9. [Servicios Implementados](#-servicios-implementados)
10. [Configuración](#-configuración)
11. [Ejemplos de Uso con cURL](#-ejemplos-de-uso-con-curl)

---

## ??? Arquitectura General

El sistema sigue la arquitectura **Clean Architecture** con patrón **CQRS** usando **MediatR**:

```mermaid
graph TB
    subgraph "Capa de Presentación"
        A[Web API - Endpoints]
        B[Middleware IpBlocking]
    end
    
    subgraph "Capa de Aplicación"
        C[Commands/Queries]
        D[Handlers]
        E[Interfaces]
    end
    
    subgraph "Capa de Dominio"
        F[Entidades]
        G[Enums]
    end
    
    subgraph "Capa de Infraestructura"
        H[Servicios]
        I[EF Core DbContext]
        J[Identity]
        K[Background Jobs]
    end
    
    subgraph "Base de Datos"
        L[(SQL Server)]
    end
    
    A --> B
    B --> C
    C --> D
    D --> E
    E --> H
    H --> I
    H --> J
    I --> L
    K --> H
    F --> I
    G --> F
```

---

## ?? Diagrama de Entidad-Relación

```mermaid
erDiagram
    AspNetUsers ||--o{ RefreshTokens : "tiene"
    AspNetUsers ||--o{ AuditLogs : "genera"
    AspNetUsers ||--o{ LoginAttempts : "realiza"
    AspNetUsers }o--o{ AspNetRoles : "pertenece"
    AspNetRoles ||--o{ RolePermissions : "tiene"
    RolePermissions }o--|| Permissions : "referencia"
    
    AspNetUsers {
        string Id PK
        string UserName
        string Email
        string PasswordHash
        bool IsTwoFactorEnabled
        datetime LastLoginDate
        string LastLoginIp
        int FailedLoginAttempts
        datetime LastFailedLoginAttempt
        datetime LockoutEnd
        bool LockoutEnabled
    }
    
    RefreshTokens {
        int Id PK
        string Token UK
        string UserId FK
        datetime ExpiryDate
        datetime CreatedDate
        bool IsRevoked
        datetime RevokedDate
        string ReplacedByToken
        string CreatedByIp
        string RevokedByIp
        string ReasonRevoked
    }
    
    IpBlackLists {
        int Id PK
        string IpAddress
        string Reason
        datetime BlockedDate
        datetime ExpiryDate
        string BlockedBy
        string Notes
        int BlackListReason
    }
    
    LoginAttempts {
        int Id PK
        string Email
        string IpAddress
        datetime AttemptDate
        bool WasSuccessful
        string FailureReason
        int FailureReasonType
        string UserAgent
    }
    
    AuditLogs {
        int Id PK
        string UserId FK
        string Action
        datetime Timestamp
        string IpAddress
        string UserAgent
        string Details
        string Status
    }
    
    Permissions {
        int Id PK
        string Name
        string Description
        int Module
    }
    
    RolePermissions {
        int Id PK
        string RoleId FK
        int PermissionId FK
    }
    
    AspNetRoles {
        string Id PK
        string Name
    }
```

---

## ?? Endpoints de Autenticación

### Base URL: `/api/Authentication`

| Método | Endpoint | Descripción | Auth Requerida |
|--------|----------|-------------|----------------|
| `POST` | `/register` | Registro de nuevo usuario | ? No |
| `POST` | `/login` | Inicio de sesión | ? No |
| `POST` | `/verify-2fa` | Verificar código 2FA | ? No |
| `POST` | `/refresh-token` | Renovar access token | ? No |
| `POST` | `/logout` | Cerrar sesión (revoca refresh tokens) | ? Sí |
| `POST` | `/revoke-token` | Revocar refresh token específico | ? Sí |
| `POST` | `/forgot-password` | Solicitar reset de contraseña | ? No |
| `POST` | `/reset-password` | Restablecer contraseña | ? No |
| `POST` | `/enable-2fa` | Habilitar autenticación 2FA | ? Sí |
| `GET` | `/audit-logs` | Obtener logs de auditoría (admin) | ? Admin |
| `GET` | `/audit-logs/user/{userId}` | Logs de un usuario específico | ? Sí |

---

## ?? Endpoints de Seguridad (Admin)

### Base URL: `/api/Security`

| Método | Endpoint | Descripción | Auth Requerida |
|--------|----------|-------------|----------------|
| `GET` | `/blocked-ips` | Listar IPs bloqueadas | ? Admin |
| `POST` | `/block-ip` | Bloquear una IP manualmente | ? Admin |
| `POST` | `/unblock-ip` | Desbloquear una IP | ? Admin |
| `POST` | `/unlock-account` | Desbloquear cuenta de usuario | ? Admin |

---

## ?? Flujo Completo de Login

```mermaid
sequenceDiagram
    participant C as Cliente
    participant MW as IpBlockingMiddleware
    participant API as LoginHandler
    participant IPBL as IpBlackListService
    participant LA as LoginAttemptService
    participant AL as AccountLockService
    participant AUTH as AuthOperations
    participant 2FA as TwoFactorService
    participant JWT as JwtTokenGenerator
    participant RT as RefreshTokenService
    participant AUDIT as AuditLogService
    participant DB as Database

    C->>MW: POST /login {email, password, ipAddress}
    
    Note over MW,IPBL: 1. Verificar IP no bloqueada
    MW->>IPBL: IsIpBlockedAsync(ipAddress)
    IPBL->>DB: SELECT FROM IpBlackLists
    IPBL-->>MW: false
    MW->>API: Continuar
    
    Note over API,LA: 2. Rate Limiting por IP
    API->>LA: GetRecentAttemptsByIpAsync(ip, 15min)
    LA->>DB: COUNT LoginAttempts
    LA-->>API: 3 intentos
    
    Note over API,AUTH: 3. Verificar credenciales
    API->>AUTH: VerifyCredentialsAsync(email, password)
    AUTH->>DB: Verificar hash de contraseña
    AUTH-->>API: {isValid: true, userId, email}
    
    Note over API,AL: 4. Verificar cuenta no bloqueada
    API->>AL: GetAccountLockStatusAsync(userId)
    AL->>DB: SELECT LockoutEnd FROM AspNetUsers
    AL-->>API: {isLocked: false}
    
    Note over API,2FA: 5. Verificar si tiene 2FA
    API->>2FA: IsTwoFactorEnabledAsync(userId)
    2FA->>DB: SELECT IsTwoFactorEnabled
    2FA-->>API: true
    
    Note over API,JWT: 6. Generar token temporal
    API->>JWT: GenerateToken(userId, requires2FA: true)
    JWT-->>API: tempToken
    
    Note over API,2FA: 7. Enviar código 2FA
    API->>2FA: GenerateAndSendTwoFactorCodeAsync(userId)
    2FA->>DB: UPDATE TwoFactorCode
    2FA->>Email: SendTwoFactorCodeAsync(email, code)
    2FA-->>API: true
    
    Note over API,LA: 8. Registrar intento exitoso
    API->>LA: RecordAttemptAsync(email, ip, true)
    LA->>DB: INSERT INTO LoginAttempts
    
    Note over API,AUDIT: 9. Auditar acción
    API->>AUDIT: LogActionAsync("LOGIN_PENDING_2FA")
    AUDIT->>DB: INSERT INTO AuditLogs
    
    API-->>C: {success: true, token: tempToken, requires2FA: true}
    
    Note over C,API: Usuario ingresa código 2FA
    C->>API: POST /verify-2fa {code, token}
    
    Note over API,2FA: 10. Validar código 2FA
    API->>2FA: ValidateTwoFactorCodeAsync(userId, code)
    2FA->>DB: SELECT TwoFactorCode, Expiration
    2FA-->>API: true
    
    Note over API,JWT: 11. Generar token final
    API->>JWT: GenerateToken(userId, requires2FA: false)
    JWT-->>API: finalToken
    
    Note over API,RT: 12. Generar Refresh Token
    API->>RT: GenerateRefreshTokenAsync(userId, ip)
    RT->>DB: INSERT INTO RefreshTokens
    RT-->>API: refreshToken
    
    Note over API,AL: 13. Resetear contadores
    API->>AL: ResetFailedAttemptsAsync(userId)
    AL->>DB: UPDATE FailedLoginAttempts = 0
    
    Note over API,AUDIT: 14. Auditar login exitoso
    API->>AUDIT: LogActionAsync("LOGIN_SUCCESS")
    AUDIT->>DB: INSERT INTO AuditLogs
    
    API-->>C: {success: true, token: finalToken, refreshToken, user}
```

---

## ?? Flujo de Refresh Token

```mermaid
sequenceDiagram
    participant C as Cliente
    participant MW as IpBlockingMiddleware
    participant API as RefreshTokenHandler
    participant IPBL as IpBlackListService
    participant RT as RefreshTokenService
    participant JWT as JwtTokenGenerator
    participant DB as Database

    C->>MW: POST /refresh-token {refreshToken, ipAddress}
    
    Note over MW,IPBL: 1. Verificar IP
    MW->>IPBL: IsIpBlockedAsync(ipAddress)
    IPBL-->>MW: false
    MW->>API: Continuar
    
    Note over API,RT: 2. Validar Refresh Token
    API->>RT: ValidateRefreshTokenAsync(token, ip)
    RT->>DB: SELECT * FROM RefreshTokens WHERE Token = @token
    RT->>DB: Verificar: !IsRevoked AND ExpiryDate > NOW()
    RT-->>API: {isValid: true, userId, email}
    
    Note over API,JWT: 3. Generar nuevo Access Token
    API->>JWT: GenerateToken(userId, email, roles)
    JWT-->>API: newAccessToken
    
    Note over API,RT: 4. Rotar Refresh Token
    API->>RT: GenerateRefreshTokenAsync(userId, ip)
    RT->>DB: INSERT INTO RefreshTokens (nuevo)
    RT-->>API: newRefreshToken
    
    Note over API,RT: 5. Revocar token anterior
    API->>RT: RevokeRefreshTokenAsync(oldToken, ip)
    RT->>DB: UPDATE RefreshTokens SET IsRevoked = 1, ReplacedByToken = @newToken
    
    API-->>C: {token: newAccessToken, refreshToken: newRefreshToken}
```

---

## ??? Protección Anti Fuerza Bruta

### Múltiples Capas de Seguridad

```mermaid
graph TD
    A[Intento de Login] --> B{IP Bloqueada?}
    B -->|Sí| C[HTTP 403 - Acceso Denegado]
    B -->|No| D{Rate Limit IP?}
    D -->|>5 intentos en 15min| E[Rechazar - Demasiados intentos]
    D -->|OK| F{Verificar Credenciales}
    F -->|Inválidas| G[Incrementar Contador]
    F -->|Válidas| H{Cuenta Bloqueada?}
    G --> I{>10 intentos en 1h?}
    I -->|Sí| J[Bloquear IP 24h]
    I -->|No| K{>5 intentos fallidos?}
    K -->|Sí| L[Bloquear cuenta 15min]
    K -->|No| E
    H -->|Sí| M[Rechazar - Cuenta bloqueada]
    H -->|No| N[Login Exitoso]
    N --> O[Resetear Contadores]
    O --> P[Generar Tokens]
```

### Configuración de Seguridad

```json
{
  "Security": {
    "RefreshToken": {
      "ExpirationDays": 7,
      "MaxActiveTokensPerUser": 5
    },
    "RateLimiting": {
      "MaxAttemptsPerIp": 5,
      "WindowMinutes": 15,
      "IpBlockDurationMinutes": 60
    },
    "AccountLock": {
      "MaxFailedAttempts": 5,
      "LockoutMinutes": 15,
      "ResetFailedAttemptsAfterMinutes": 60
    },
    "BlackList": {
      "EnableAutoBlock": true,
      "AutoBlockAfterAttempts": 10,
      "AutoBlockDurationHours": 24
    }
  }
}
```

---

## ??? Entidades de Base de Datos

### RefreshToken

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `Id` | int | PK, Identity |
| `Token` | string(500) | Token único (base64) |
| `UserId` | string(450) | FK a AspNetUsers |
| `ExpiryDate` | datetime | Fecha de expiración |
| `CreatedDate` | datetime | Fecha de creación |
| `IsRevoked` | bool | Si fue revocado |
| `RevokedDate` | datetime? | Cuándo fue revocado |
| `ReplacedByToken` | string(500)? | Token que lo reemplazó |
| `CreatedByIp` | string(45)? | IP de creación |
| `RevokedByIp` | string(45)? | IP de revocación |
| `ReasonRevoked` | string(500)? | Razón de revocación |

### IpBlackList

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `Id` | int | PK, Identity |
| `IpAddress` | string(45) | Dirección IP |
| `Reason` | string(500) | Razón del bloqueo |
| `BlockedDate` | datetime | Cuándo se bloqueó |
| `ExpiryDate` | datetime? | Cuándo expira (null = permanente) |
| `BlockedBy` | string(450)? | Quién bloqueó |
| `Notes` | string(1000)? | Notas adicionales |
| `BlackListReason` | enum | ManualBlock, TooManyAttempts, SuspiciousActivity, ReportedAbuse |

### LoginAttempt

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `Id` | int | PK, Identity |
| `Email` | string(256) | Email del intento |
| `IpAddress` | string(45) | IP del intento |
| `AttemptDate` | datetime | Cuándo ocurrió |
| `WasSuccessful` | bool | Si fue exitoso |
| `FailureReason` | string(500)? | Razón del fallo |
| `FailureReasonType` | enum? | InvalidCredentials, AccountLocked, IpBlocked, etc. |
| `UserAgent` | string(500)? | User agent del navegador |

### ApplicationUser (extends IdentityUser)

```csharp
public class ApplicationUser : IdentityUser
{
    public bool IsTwoFactorEnabled { get; set; }
    public DateTime? LastLoginDate { get; set; }
    public string? LastLoginIp { get; set; }
    public int FailedLoginAttempts { get; set; }
    public DateTime? LastFailedLoginAttempt { get; set; }
    
    // Relaciones
    public ICollection<AuditLog> AuditLogs { get; set; }
}
```

---

## ?? Servicios Implementados

### 1. IRefreshTokenService

```csharp
Task<RefreshToken> GenerateRefreshTokenAsync(string userId, string ipAddress);
Task<(bool isValid, string? userId, string? email)> ValidateRefreshTokenAsync(string token, string ipAddress);
Task<bool> RevokeRefreshTokenAsync(string token, string ipAddress, string reason);
Task<int> RevokeAllUserTokensAsync(string userId, string ipAddress, string reason);
Task<IEnumerable<RefreshToken>> GetActiveTokensByUserAsync(string userId);
Task<int> CleanupExpiredTokensAsync();
```

### 2. IIpBlackListService

```csharp
Task<bool> IsIpBlockedAsync(string ipAddress);
Task<IpBlackList> BlockIpAsync(string ipAddress, string reason, BlackListReason type, string? blockedBy, DateTime? expiryDate, string? notes);
Task<bool> UnblockIpAsync(string ipAddress, string unblockedBy);
Task<IEnumerable<IpBlackList>> GetBlockedIpsAsync(bool activeOnly = true);
Task<int> CleanupExpiredBlocksAsync();
Task<IpBlackList?> GetBlockInfoAsync(string ipAddress);
```

### 3. ILoginAttemptService

```csharp
Task RecordAttemptAsync(string email, string ipAddress, bool wasSuccessful, string? failureReason, LoginFailureReason? type, string? userAgent);
Task<int> GetRecentAttemptsByIpAsync(string ipAddress, int minutes = 15);
Task<int> GetFailedAttemptsByEmailAsync(string email, int minutes = 60);
Task<bool> ShouldBlockIpAsync(string ipAddress);
Task<bool> ShouldLockAccountAsync(string email);
Task<int> CleanupOldAttemptsAsync(int daysToKeep = 30);
```

### 4. IAccountLockService

```csharp
Task<bool> LockAccountAsync(string userId, int lockoutMinutes = 15);
Task<bool> UnlockAccountAsync(string userId);
Task<bool> IsAccountLockedAsync(string userId);
Task<int> IncrementFailedAttemptsAsync(string userId);
Task ResetFailedAttemptsAsync(string userId);
Task<(bool isLocked, DateTime? lockoutEnd, int failedAttempts)> GetAccountLockStatusAsync(string userId);
```

### 5. Background Job: SecurityCleanupJob

Ejecuta cada 1 hora:
- Limpia refresh tokens expirados
- Limpia bloqueos de IP expirados  
- Limpia intentos de login > 30 días

---

## ?? Configuración

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=SAPFIAIDb;..."
  },
  "Jwt": {
    "Key": "TU_CLAVE_SECRETA_MUY_LARGA_Y_SEGURA",
    "Issuer": "SAPFIAI",
    "Audience": "SAPFIAI-Users",
    "ExpireMinutes": "15"
  },
  "Security": {
    "RefreshToken": {
      "ExpirationDays": 7,
      "MaxActiveTokensPerUser": 5
    },
    "RateLimiting": {
      "MaxAttemptsPerIp": 5,
      "WindowMinutes": 15
    },
    "AccountLock": {
      "MaxFailedAttempts": 5,
      "LockoutMinutes": 15
    },
    "BlackList": {
      "EnableAutoBlock": true,
      "AutoBlockAfterAttempts": 10,
      "AutoBlockDurationHours": 24
    }
  },
  "Brevo": {
    "ApiKey": "TU_API_KEY_DE_BREVO",
    "SenderEmail": "noreply@tudominio.com",
    "SenderName": "SAPFIAI"
  }
}
```

### Variables de Entorno (.env)

```bash
# Base de datos
ConnectionStrings__DefaultConnection="Server=...;Database=SAPFIAIDb;..."

# JWT
Jwt__Key="TU_CLAVE_SECRETA_MUY_LARGA_Y_SEGURA"
Jwt__Issuer="SAPFIAI"
Jwt__Audience="SAPFIAI-Users"
Jwt__ExpireMinutes="15"

# Brevo Email
Brevo__ApiKey="xkeysib-..."
Brevo__SenderEmail="noreply@tudominio.com"
Brevo__SenderName="SAPFIAI"

# Seguridad
Security__RefreshToken__ExpirationDays="7"
Security__RateLimiting__MaxAttemptsPerIp="5"
Security__AccountLock__MaxFailedAttempts="5"
```

---

## ?? Ejemplos de Uso con cURL

### 1. Registro de Usuario

```bash
curl -X POST https://localhost:5001/api/Authentication/register \
  -H "Content-Type: application/json" \
  -d '{
    "userName": "juan.perez",
    "email": "juan.perez@example.com",
    "password": "Pass123!@#",
    "confirmPassword": "Pass123!@#"
  }'
```

**Respuesta exitosa:**
```json
{
  "success": true,
  "userId": "abc123...",
  "message": "Usuario registrado exitosamente"
}
```

### 2. Login (con 2FA habilitado)

```bash
curl -X POST https://localhost:5001/api/Authentication/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "juan.perez@example.com",
    "password": "Pass123!@#"
  }'
```

**Respuesta (requiere 2FA):**
```json
{
  "success": true,
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "requires2FA": true,
  "message": "Código de verificación enviado a tu correo"
}
```

### 3. Verificar Código 2FA

```bash
curl -X POST https://localhost:5001/api/Authentication/verify-2fa \
  -H "Content-Type: application/json" \
  -d '{
    "code": "123456",
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
  }'
```

**Respuesta exitosa:**
```json
{
  "success": true,
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "xW3k9pLmN7vQ2rT8yU4jK5sF1hG6dA0zC...",
  "refreshTokenExpiry": "2024-02-24T10:30:00Z",
  "user": {
    "id": "abc123...",
    "userName": "juan.perez",
    "email": "juan.perez@example.com"
  }
}
```

### 4. Refresh Token

```bash
curl -X POST https://localhost:5001/api/Authentication/refresh-token \
  -H "Content-Type: application/json" \
  -d '{
    "refreshToken": "xW3k9pLmN7vQ2rT8yU4jK5sF1hG6dA0zC..."
  }'
```

**Respuesta:**
```json
{
  "success": true,
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "yN2m8qOlP6wR3sU9zV5kL4tG2iH7eB1aC...",
  "refreshTokenExpiry": "2024-02-24T10:30:00Z"
}
```

### 5. Logout

```bash
curl -X POST https://localhost:5001/api/Authentication/logout \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

**Respuesta:**
```json
{
  "succeeded": true
}
```

### 6. Bloquear IP (Admin)

```bash
curl -X POST https://localhost:5001/api/Security/block-ip \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." \
  -H "Content-Type: application/json" \
  -d '{
    "ipAddress": "192.168.1.100",
    "reason": "Actividad sospechosa detectada",
    "blackListReason": 2,
    "expiryDate": "2024-02-20T23:59:59Z",
    "notes": "Múltiples intentos de login fallidos"
  }'
```

### 7. Ver Logs de Auditoría (Admin)

```bash
curl -X GET "https://localhost:5001/api/Authentication/audit-logs?pageNumber=1&pageSize=20&action=LOGIN_SUCCESS" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

---

## ?? Seguridad Implementada

### ? Características de Seguridad

1. **Refresh Token con Rotación**
   - Tokens de 7 días de duración
   - Rotación automática al renovar
   - Máximo 5 tokens activos por usuario
   - Revocación en cascada

2. **Rate Limiting por IP**
   - Máximo 5 intentos en 15 minutos
   - Bloqueo temporal de 60 minutos
   - Tracking de todos los intentos

3. **Black List de IPs**
   - Bloqueo manual de IPs sospechosas
   - Bloqueo automático tras 10 intentos fallidos
   - Bloqueos temporales o permanentes

4. **Bloqueo de Cuentas**
   - Bloqueo tras 5 intentos fallidos
   - Lockout de 15 minutos
   - Reseteo automático de contadores

5. **Auditoría Completa**
   - Todos los eventos registrados
   - IP y User-Agent capturados
   - Búsqueda por usuario, acción, fecha

6. **Background Jobs**
   - Limpieza automática de tokens expirados
   - Limpieza de bloqueos expirados
   - Limpieza de logs antiguos

---

## ?? Notas Técnicas

### Middleware Order

El orden de los middlewares es **crítico**:

```csharp
app.UseHealthChecks("/health");
app.UseHttpsRedirection();
app.UseStaticFiles();

// ?? DEBE estar ANTES de Authentication
app.UseMiddleware<IpBlockingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();
```

### Migración de Base de Datos

```bash
# Crear migración
dotnet ef migrations add AddSecurityFeatures --project src/Infrastructure --startup-project src/Web

# Aplicar migración
dotnet ef database update --project src/Infrastructure --startup-project src/Web
```

---

## ?? Referencias

- [ASP.NET Core Identity](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity)
- [JWT Bearer Authentication](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/jwt-bearer)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [CQRS Pattern](https://docs.microsoft.com/en-us/azure/architecture/patterns/cqrs)
- [MediatR](https://github.com/jbogard/MediatR)

---

**Última actualización:** 17/02/2024  
**Versión:** 2.0.0
