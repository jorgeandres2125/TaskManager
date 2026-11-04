/*
    TaskManager - 03. Carga masiva de datos de prueba.
    Completa la base hasta @TargetUsers usuarios y @TargetTasks tareas (por defecto 200 y 1000).
    Es idempotente: si ya se alcanzaron los totales, no inserta nada.
    Requiere SQL Server 2022 (JSON_OBJECT / JSON_ARRAY) y haber ejecutado 01_schema.sql.
    Ejecutar con: sqlcmd ... -I -f 65001 -d <base> -i 03_seed_bulk.sql

    Las tareas generadas llevan "metadata": {"source": "seed-bulk"} para poder identificarlas.
    Para eliminar todo lo generado por este script, ver la sección "Limpieza" al final.
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @TargetUsers INT = 200;
DECLARE @TargetTasks INT = 1000;

DECLARE @UsersToInsert INT = @TargetUsers - (SELECT COUNT(*) FROM dbo.Users);
DECLARE @TasksToInsert INT = @TargetTasks - (SELECT COUNT(*) FROM dbo.Tasks);
DECLARE @Now DATETIME2 = SYSUTCDATETIME();

BEGIN TRANSACTION;

-------------------------------------------------------------------------------
-- Usuarios: combinaciones de nombre y apellido; el correo lleva un número para ser único.
-------------------------------------------------------------------------------
IF @UsersToInsert > 0
BEGIN
    DECLARE @FirstNames TABLE (Name NVARCHAR(50));
    INSERT INTO @FirstNames VALUES
        (N'Ana'), (N'Carlos'), (N'Laura'), (N'Andrés'), (N'Sofía'), (N'Miguel'), (N'Valentina'), (N'Juan'),
        (N'Camila'), (N'Diego'), (N'Isabella'), (N'Felipe'), (N'Daniela'), (N'Santiago'), (N'Mariana'),
        (N'Sebastián'), (N'Paula'), (N'Nicolás'), (N'Lucía'), (N'Tomás');

    DECLARE @LastNames TABLE (Name NVARCHAR(50));
    INSERT INTO @LastNames VALUES
        (N'García'), (N'Rodríguez'), (N'Martínez'), (N'López'), (N'González'), (N'Pérez'), (N'Sánchez'),
        (N'Ramírez'), (N'Torres'), (N'Flores'), (N'Rivera'), (N'Gómez'), (N'Díaz'), (N'Reyes'), (N'Morales'),
        (N'Ortiz'), (N'Castro'), (N'Vargas'), (N'Rojas'), (N'Herrera');

    ;WITH Candidates AS (
        SELECT
            f.Name AS FirstName,
            l.Name AS LastName,
            ROW_NUMBER() OVER (ORDER BY CHECKSUM(f.Name, l.Name)) AS Seq
        FROM @FirstNames AS f
        CROSS JOIN @LastNames AS l
    )
    INSERT INTO dbo.Users (Id, Name, Email, CreatedAt)
    SELECT TOP (@UsersToInsert)
        NEWID(),
        c.FirstName + N' ' + c.LastName,
        -- Correo sin tildes, en minúsculas, como lo normaliza la aplicación.
        TRANSLATE(LOWER(c.FirstName + N'.' + c.LastName), N'áéíóúñü', N'aeiounu')
            + N'.' + RIGHT(N'000' + CAST(c.Seq AS NVARCHAR(10)), 3) + N'@acme.com',
        DATEADD(SECOND, -ABS(CHECKSUM(NEWID())) % (180 * 24 * 3600), @Now)   -- últimos 180 días
    FROM Candidates AS c
    WHERE NOT EXISTS (
        SELECT 1 FROM dbo.Users AS u
        WHERE u.Email = TRANSLATE(LOWER(c.FirstName + N'.' + c.LastName), N'áéíóúñü', N'aeiounu')
            + N'.' + RIGHT(N'000' + CAST(c.Seq AS NVARCHAR(10)), 3) + N'@acme.com')
    ORDER BY c.Seq;
END;

-------------------------------------------------------------------------------
-- Tareas: título (verbo + objeto), usuario, estado, fechas e información adicional en JSON.
-------------------------------------------------------------------------------
IF @TasksToInsert > 0
BEGIN
    DECLARE @Verbs TABLE (Id INT IDENTITY(0, 1), Verb NVARCHAR(30));
    INSERT INTO @Verbs (Verb) VALUES
        (N'Revisar'), (N'Actualizar'), (N'Documentar'), (N'Configurar'), (N'Corregir'),
        (N'Diseñar'), (N'Implementar'), (N'Probar'), (N'Migrar'), (N'Optimizar');

    DECLARE @Objects TABLE (Id INT IDENTITY(0, 1), Object NVARCHAR(60));
    INSERT INTO @Objects (Object) VALUES
        (N'el módulo de facturación'), (N'la API de clientes'), (N'el reporte mensual'), (N'el tablero de ventas'),
        (N'la integración con pagos'), (N'el flujo de aprobación'), (N'la base de datos de inventario'),
        (N'el portal de proveedores'), (N'las notificaciones por correo'), (N'el proceso de despliegue'),
        (N'la pantalla de inicio'), (N'el servicio de autenticación'), (N'los índices de consultas'),
        (N'el manual de usuario'), (N'la exportación a Excel');

    -- Conjuntos de etiquetas como arreglos JSON (1 a 3 etiquetas por tarea).
    DECLARE @TagSets TABLE (Id INT IDENTITY(0, 1), Tags NVARCHAR(200));
    INSERT INTO @TagSets (Tags) VALUES
        (JSON_ARRAY(N'backend')), (JSON_ARRAY(N'frontend')), (JSON_ARRAY(N'database')),
        (JSON_ARRAY(N'backend', N'api')), (JSON_ARRAY(N'frontend', N'ux')), (JSON_ARRAY(N'devops', N'backend')),
        (JSON_ARRAY(N'docs')), (JSON_ARRAY(N'database', N'performance')), (JSON_ARRAY(N'bug', N'backend')),
        (JSON_ARRAY(N'bug', N'frontend', N'urgent')), (JSON_ARRAY(N'security', N'backend')), (JSON_ARRAY(N'qa'));

    DECLARE @Users TABLE (Seq INT IDENTITY(0, 1), Id UNIQUEIDENTIFIER);
    INSERT INTO @Users (Id) SELECT Id FROM dbo.Users;
    DECLARE @UserCount INT = (SELECT COUNT(*) FROM @Users);

    DECLARE @VerbCount INT = (SELECT COUNT(*) FROM @Verbs);
    DECLARE @ObjectCount INT = (SELECT COUNT(*) FROM @Objects);
    DECLARE @TagSetCount INT = (SELECT COUNT(*) FROM @TagSets);

    -- Los valores aleatorios se materializan primero: si NEWID() quedara dentro de un CTE usado varias veces,
    -- SQL Server podría recalcularlo y una misma fila tendría, por ejemplo, dos CreatedAt distintas.
    DECLARE @Random TABLE (
        N INT PRIMARY KEY, RUser INT, RTitle INT, RStatus INT, RPriority INT, RTags INT,
        RAgeSeconds INT, REstimateDays INT, RUpdateHours INT);

    ;WITH Numbers AS (
        SELECT TOP (@TasksToInsert) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) - 1 AS N
        FROM sys.all_objects AS a CROSS JOIN sys.all_objects AS b
    )
    INSERT INTO @Random
    SELECT
        n.N,
        ABS(CHECKSUM(NEWID())),
        ABS(CHECKSUM(NEWID())),
        ABS(CHECKSUM(NEWID())) % 100,
        ABS(CHECKSUM(NEWID())) % 100,
        ABS(CHECKSUM(NEWID())),
        ABS(CHECKSUM(NEWID())) % (120 * 24 * 3600),   -- creadas en los últimos 120 días
        ABS(CHECKSUM(NEWID())) % 30 + 1,
        ABS(CHECKSUM(NEWID())) % 72 + 1
    FROM Numbers AS n;

    ;WITH Shaped AS (
        SELECT
            r.*,
            DATEADD(SECOND, -r.RAgeSeconds, @Now) AS CreatedAt,
            CASE WHEN r.RStatus < 40 THEN N'Pending' WHEN r.RStatus < 70 THEN N'InProgress' ELSE N'Done' END AS Status,
            CASE WHEN r.RPriority < 15 THEN NULL                      -- 15 % sin prioridad
                 WHEN r.RPriority < 45 THEN N'Low'
                 WHEN r.RPriority < 80 THEN N'Medium'
                 ELSE N'High' END AS Priority
        FROM @Random AS r
    )
    INSERT INTO dbo.Tasks (Id, Title, Status, UserId, AdditionalInfo, CreatedAt, UpdatedAt)
    SELECT
        NEWID(),
        v.Verb + N' ' + o.Object,   -- el número (#N) lo asigna la columna Tasks.Number (secuencia)
        s.Status,
        u.Id,
        JSON_OBJECT(
            'priority': s.Priority,
            'estimatedCompletionDate': CONVERT(CHAR(10), DATEADD(DAY, s.REstimateDays, s.CreatedAt), 23),
            'tags': JSON_QUERY(t.Tags),
            'metadata': JSON_OBJECT('source': N'seed-bulk', 'sprint': CAST(s.N % 12 + 1 AS NVARCHAR(10)))
            ABSENT ON NULL),
        s.CreatedAt,
        -- Las tareas que ya pasaron por InProgress o Done tienen fecha de actualización.
        CASE WHEN s.Status = N'Pending' THEN NULL
             ELSE (SELECT MIN(d) FROM (VALUES (DATEADD(HOUR, s.RUpdateHours, s.CreatedAt)), (@Now)) AS x(d)) END
    FROM Shaped AS s
    INNER JOIN @Users   AS u ON u.Seq = s.RUser % @UserCount
    INNER JOIN @Verbs   AS v ON v.Id  = s.RTitle % @VerbCount
    INNER JOIN @Objects AS o ON o.Id  = (s.RTitle / @VerbCount) % @ObjectCount
    INNER JOIN @TagSets AS t ON t.Id  = s.RTags % @TagSetCount;
END;

COMMIT TRANSACTION;

-------------------------------------------------------------------------------
-- Resumen
-------------------------------------------------------------------------------
SELECT (SELECT COUNT(*) FROM dbo.Users) AS Users, (SELECT COUNT(*) FROM dbo.Tasks) AS Tasks;

SELECT Status, COUNT(*) AS Tasks FROM dbo.Tasks GROUP BY Status ORDER BY Status;

SELECT ISNULL(Priority, N'(sin prioridad)') AS Priority, COUNT(*) AS Tasks FROM dbo.Tasks GROUP BY Priority ORDER BY Priority;

/*
-------------------------------------------------------------------------------
-- Limpieza (opcional): elimina solo lo generado por este script.
-------------------------------------------------------------------------------
DELETE FROM dbo.Tasks WHERE JSON_VALUE(AdditionalInfo, '$.metadata.source') = N'seed-bulk';
DELETE u FROM dbo.Users AS u
WHERE u.Email LIKE N'%.[0-9][0-9][0-9]@acme.com'
  AND NOT EXISTS (SELECT 1 FROM dbo.Tasks AS t WHERE t.UserId = u.Id);
*/
