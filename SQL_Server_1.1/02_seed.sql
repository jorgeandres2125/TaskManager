/*
    TaskManager - 02. Datos de ejemplo.
    Requiere haber ejecutado 01_schema.sql. Es idempotente: no duplica usuarios ni tareas.
    Las tareas en estado Done se insertan directamente; en la aplicación solo se llega a Done desde InProgress.
*/
SET NOCOUNT ON;

DECLARE @Users TABLE (Name NVARCHAR(100), Email NVARCHAR(256));
INSERT INTO @Users (Name, Email) VALUES
    (N'Ana Gómez',      N'ana.gomez@acme.com'),
    (N'Carlos Pérez',   N'carlos.perez@acme.com'),
    (N'Laura Martínez', N'laura.martinez@acme.com');

INSERT INTO dbo.Users (Id, Name, Email, CreatedAt)
SELECT NEWID(), s.Name, s.Email, SYSUTCDATETIME()
FROM @Users AS s
WHERE NOT EXISTS (SELECT 1 FROM dbo.Users AS u WHERE u.Email = s.Email);

DECLARE @Tasks TABLE (Title NVARCHAR(200), Status NVARCHAR(20), Email NVARCHAR(256), AdditionalInfo NVARCHAR(MAX), DaysAgo INT);
INSERT INTO @Tasks (Title, Status, Email, AdditionalInfo, DaysAgo) VALUES
    (N'Configurar pipeline de CI',        N'Pending',    N'ana.gomez@acme.com',
        N'{"priority":"High","estimatedCompletionDate":"2026-11-20","tags":["devops","backend"],"metadata":{"repo":"taskmanager-api"}}', 5),
    (N'Diseñar pantalla de tareas',       N'InProgress', N'laura.martinez@acme.com',
        N'{"priority":"Medium","estimatedCompletionDate":"2026-11-15","tags":["frontend","ux"],"metadata":{"figma":"v2"}}', 4),
    (N'Revisar índices de la tabla Tasks', N'Done',      N'carlos.perez@acme.com',
        N'{"priority":"Low","tags":["database"],"metadata":{"ticket":"DB-42"}}', 3),
    (N'Documentar API en el README',      N'Pending',    N'carlos.perez@acme.com',
        N'{"priority":"Medium","tags":["docs","backend"]}', 2),
    (N'Preparar demo para el cliente',    N'InProgress', N'ana.gomez@acme.com',
        NULL, 1);

INSERT INTO dbo.Tasks (Id, Title, Status, UserId, AdditionalInfo, CreatedAt, UpdatedAt)
SELECT NEWID(), s.Title, s.Status, u.Id, s.AdditionalInfo, DATEADD(DAY, -s.DaysAgo, SYSUTCDATETIME()),
       -- Las tareas que ya cambiaron de estado tienen fecha de actualización.
       CASE WHEN s.Status = N'Pending' THEN NULL ELSE DATEADD(HOUR, 4, DATEADD(DAY, -s.DaysAgo, SYSUTCDATETIME())) END
FROM @Tasks AS s
INNER JOIN dbo.Users AS u ON u.Email = s.Email
WHERE NOT EXISTS (SELECT 1 FROM dbo.Tasks AS t WHERE t.Title = s.Title AND t.UserId = u.Id);

SELECT (SELECT COUNT(*) FROM dbo.Users) AS Users, (SELECT COUNT(*) FROM dbo.Tasks) AS Tasks;
