namespace SAPFIAI.Domain.Enums;

/// <summary>
/// Tipos de acción para auditoría
/// </summary>
public enum AuditActionType
{
    // Autenticación
    Login,
    LoginFailed,
    Logout,
    Register,
    PasswordChanged,
    PasswordReset,
    TwoFactorEnabled,
    TwoFactorDisabled,
    TwoFactorValidated,
    TwoFactorFailed,
    RefreshToken,
    TokenRevoked,

    // Usuarios
    UserCreated,
    UserUpdated,
    UserDeleted,
    UserActivated,
    UserDeactivated,

    // Roles y Permisos
    RoleCreated,
    RoleUpdated,
    RoleDeleted,
    PermissionAssigned,
    PermissionRevoked,

    // Otros
    Error,
    SecurityAlert
}

/// <summary>
/// Módulos del sistema para permisos
/// </summary>
public enum PermissionModule
{
    Authentication,
    Users,
    Roles,
    Permissions,
    AuditLogs,
    System
}

/// <summary>
/// Estados de usuario
/// </summary>
public enum UserStatus
{
    Active,
    Inactive,
    Locked,
    PendingVerification,
    Suspended
}

/// <summary>
/// Razones de bloqueo de IP
/// </summary>
public enum BlackListReason
{
    ManualBlock,
    TooManyAttempts,
    SuspiciousActivity,
    ReportedAbuse
}

/// <summary>
/// Razones de fallo de login
/// </summary>
public enum LoginFailureReason
{
    InvalidCredentials,
    AccountLocked,
    IpBlocked,
    TwoFactorRequired,
    AccountNotVerified
}

