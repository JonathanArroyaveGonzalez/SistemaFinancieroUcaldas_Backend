-- =============================================================================
-- Script para eliminar todas las tablas de la base de datos SAPFIAI
-- Ejecutar este script ANTES de aplicar las migraciones
-- =============================================================================

-- Deshabilitar restricciones de clave foránea temporalmente
EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL';
GO

-- Eliminar todas las tablas en orden correcto (evitar errores de FK)

-- Tablas de auditoría y tokens
IF OBJECT_ID('dbo.AuditLogs', 'U') IS NOT NULL DROP TABLE dbo.AuditLogs;
IF OBJECT_ID('dbo.RefreshTokens', 'U') IS NOT NULL DROP TABLE dbo.RefreshTokens;

-- Tablas de permisos
IF OBJECT_ID('dbo.RolePermissions', 'U') IS NOT NULL DROP TABLE dbo.RolePermissions;
IF OBJECT_ID('dbo.Permissions', 'U') IS NOT NULL DROP TABLE dbo.Permissions;

-- Tablas de Identity
IF OBJECT_ID('dbo.AspNetUserTokens', 'U') IS NOT NULL DROP TABLE dbo.AspNetUserTokens;
IF OBJECT_ID('dbo.AspNetUserRoles', 'U') IS NOT NULL DROP TABLE dbo.AspNetUserRoles;
IF OBJECT_ID('dbo.AspNetUserLogins', 'U') IS NOT NULL DROP TABLE dbo.AspNetUserLogins;
IF OBJECT_ID('dbo.AspNetUserClaims', 'U') IS NOT NULL DROP TABLE dbo.AspNetUserClaims;
IF OBJECT_ID('dbo.AspNetRoleClaims', 'U') IS NOT NULL DROP TABLE dbo.AspNetRoleClaims;
IF OBJECT_ID('dbo.AspNetUsers', 'U') IS NOT NULL DROP TABLE dbo.AspNetUsers;
IF OBJECT_ID('dbo.AspNetRoles', 'U') IS NOT NULL DROP TABLE dbo.AspNetRoles;

-- Tabla de migraciones de EF Core
IF OBJECT_ID('dbo.__EFMigrationsHistory', 'U') IS NOT NULL DROP TABLE dbo.__EFMigrationsHistory;

-- Tablas antiguas (TodoList, etc.) si existen
IF OBJECT_ID('dbo.TodoItems', 'U') IS NOT NULL DROP TABLE dbo.TodoItems;
IF OBJECT_ID('dbo.TodoLists', 'U') IS NOT NULL DROP TABLE dbo.TodoLists;

PRINT 'Todas las tablas han sido eliminadas exitosamente.';
GO
