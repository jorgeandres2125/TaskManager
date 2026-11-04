/*
    TaskManager - Recrear toda la base de datos en un solo paso.

    Ejecuta, en orden:
        00_create_database.sql   crea la base (si no existe) en nivel de compatibilidad 160
        01_schema.sql            tablas, secuencia, restricciones, índices, procedimientos y migraciones de EF
        02_seed.sql              datos de ejemplo básicos (3 usuarios y 5 tareas)
        03_seed_bulk.sql         datos de prueba hasta 200 usuarios y 1000 tareas

    Todos los scripts son idempotentes: volver a ejecutarlo no duplica nada ni falla.
    04_json_queries.sql (consultas de ejemplo con funciones JSON) no forma parte de la recreación.

    Ejecutar DESDE ESTA CARPETA (:r usa rutas relativas al directorio actual), conectado a master:

        cd source_code/SQL_Server_1.1
        sqlcmd -S <servidor> -E -I -f 65001 -v DatabaseName=TaskManager -i recreate_database.sql

    Con usuario de SQL Server, cambie -E por -U <usuario> -P <clave>. -I (QUOTED_IDENTIFIER) es obligatorio:
    SQL Server lo exige para escribir en tablas con índices sobre columnas calculadas (Tasks.Priority).
*/
:on error exit

PRINT N'== 00. Base de datos';
:r 00_create_database.sql

USE [$(DatabaseName)];
GO

PRINT N'== 01. Esquema';
:r 01_schema.sql

PRINT N'== 02. Datos de ejemplo';
:r 02_seed.sql
GO

PRINT N'== 03. Datos de prueba en bloque';
:r 03_seed_bulk.sql
GO

PRINT N'== Base de datos $(DatabaseName) lista.';
