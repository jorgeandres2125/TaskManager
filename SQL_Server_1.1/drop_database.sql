/*
    TaskManager - Eliminar la base de datos (IRREVERSIBLE).

    Borra $(DatabaseName) con todos sus datos (usuarios, tareas, fotos y logs). Úselo solo para empezar de cero
    antes de recreate_database.sql. Para evitar accidentes exige confirmarlo escribiendo el nombre de la base:

        sqlcmd -S <servidor> -E -I -f 65001 -v DatabaseName=TaskManager ConfirmDrop=TaskManager -i drop_database.sql

    Cierra las conexiones abiertas (por ejemplo, la API en ejecución) antes de borrar.
*/
SET NOCOUNT ON;

IF N'$(ConfirmDrop)' <> N'$(DatabaseName)'
BEGIN
    RAISERROR(N'No se eliminó nada: indique ConfirmDrop con el mismo valor que DatabaseName para confirmar.', 16, 1);
    RETURN;
END;

IF DB_ID(N'$(DatabaseName)') IS NULL
BEGIN
    PRINT N'La base de datos $(DatabaseName) no existe: no hay nada que eliminar.';
    RETURN;
END;

ALTER DATABASE [$(DatabaseName)] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
DROP DATABASE [$(DatabaseName)];
PRINT N'Base de datos $(DatabaseName) eliminada.';
GO
