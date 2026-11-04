/*
    TaskManager - 00. Crear la base de datos.

    Crea la base $(DatabaseName) si no existe y la deja en el nivel de compatibilidad de SQL Server 2022 (160).
    El esquema necesita SQL Server 2022 o posterior: usa ISJSON(..., OBJECT), JSON_OBJECT y JSON_ARRAY.

    Ejecutar (conectado a master):
        sqlcmd -S <servidor> -E -I -f 65001 -v DatabaseName=TaskManager -i 00_create_database.sql

    En un hosting compartido, donde la base la crea el panel del proveedor y no hay permiso CREATE DATABASE,
    omita este script y empiece por 01_schema.sql sobre la base existente.
*/
SET NOCOUNT ON;

IF CAST(SERVERPROPERTY('ProductMajorVersion') AS INT) < 16
BEGIN
    RAISERROR(N'TaskManager requiere SQL Server 2022 (16.x) o posterior: el esquema usa ISJSON con tipo, JSON_OBJECT y JSON_ARRAY.', 16, 1);
    RETURN;
END;

IF DB_ID(N'$(DatabaseName)') IS NULL
BEGIN
    CREATE DATABASE [$(DatabaseName)];
    PRINT N'Base de datos $(DatabaseName) creada.';
END
ELSE
    PRINT N'La base de datos $(DatabaseName) ya existe: no se modifica.';
GO

-- Comportamiento de SQL Server 2022 (por ejemplo, para las funciones JSON y el optimizador).
IF (SELECT compatibility_level FROM sys.databases WHERE name = N'$(DatabaseName)') < 160
    ALTER DATABASE [$(DatabaseName)] SET COMPATIBILITY_LEVEL = 160;
GO
