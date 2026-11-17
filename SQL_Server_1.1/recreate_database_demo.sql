/*
    TaskManager - Recrear la base de datos con los datos EXACTOS de la demostración, en un solo paso.

    Ejecuta, en orden:
        00_create_database.sql   crea la base (si no existe) en nivel de compatibilidad 160
        01_schema.sql            tablas, secuencia, restricciones, índices, procedimientos y migraciones de EF
        05_demo_data.sql         los 200 usuarios, las 1005 tareas y las 200 fotos de la base de demostración

    Es la alternativa a recreate_database.sql (que genera datos de prueba equivalentes pero distintos).
    Es idempotente: volver a ejecutarlo no duplica nada.

    Ejecutar DESDE ESTA CARPETA (:r usa rutas relativas al directorio actual), conectado a master:

        cd source_code/SQL_Server_1.1
        sqlcmd -S <servidor> -E -I -f 65001 -v DatabaseName=TaskManager -i recreate_database_demo.sql
*/
:on error exit

PRINT N'== 00. Base de datos';
:r 00_create_database.sql

USE [$(DatabaseName)];
GO

PRINT N'== 01. Esquema';
:r 01_schema.sql

PRINT N'== 05. Datos exactos de la demostración';
:r 05_demo_data.sql

PRINT N'== Base de datos $(DatabaseName) lista.';
