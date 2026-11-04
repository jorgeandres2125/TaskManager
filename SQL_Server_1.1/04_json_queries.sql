/*
    TaskManager - Consultas de ejemplo con JSON nativo de SQL Server.
    Requiere SQL Server 2022 o superior (nivel de compatibilidad 160): ISJSON con el parámetro OBJECT
    se introdujo en SQL Server 2022. El resto de funciones existe desde SQL Server 2016.

    Columna: dbo.Tasks.AdditionalInfo NVARCHAR(MAX), con este formato:
    {
      "priority": "Low | Medium | High",
      "estimatedCompletionDate": "yyyy-MM-dd",
      "tags": ["texto", ...],
      "metadata": { "clave": "valor", ... }
    }
*/
SET NOCOUNT ON;

-------------------------------------------------------------------------------
-- 1. ISJSON: validar que el contenido sea JSON válido.
--    La tabla lo exige con la restricción CK_Tasks_AdditionalInfo_IsJson.
-------------------------------------------------------------------------------
SELECT
    ISJSON(N'{"priority":"High"}', OBJECT) AS ObjetoValido,     -- 1
    ISJSON(N'["a","b"]', OBJECT)           AS ArregloNoEsObjeto, -- 0
    ISJSON(N'{priority:High}')             AS JsonInvalido;      -- 0

-- Este INSERT falla a propósito (no guarda nada), pero consume un valor de dbo.TaskNumberSequence:
-- SQL Server calcula el valor por defecto de Number antes de comprobar la restricción. Para no dejar un hueco
-- en la numeración, al terminar reinicie la secuencia en MAX(Number) + 1 (ver README de esta carpeta).
BEGIN TRY
    INSERT INTO dbo.Tasks (Id, Title, Status, UserId, AdditionalInfo, CreatedAt)
    SELECT TOP (1) NEWID(), N'Tarea con JSON inválido', N'Pending', Id, N'{esto no es json}', SYSUTCDATETIME()
    FROM dbo.Users;
END TRY
BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber, ERROR_MESSAGE() AS RechazadoPorCheck;
END CATCH;

-------------------------------------------------------------------------------
-- 2. JSON_VALUE: consultar campos escalares del JSON.
-------------------------------------------------------------------------------
SELECT
    t.Title,
    JSON_VALUE(t.AdditionalInfo, '$.priority')                                    AS Priority,
    TRY_CONVERT(date, JSON_VALUE(t.AdditionalInfo, '$.estimatedCompletionDate')) AS EstimatedCompletionDate,
    JSON_VALUE(t.AdditionalInfo, '$.metadata.ticket')                             AS Ticket
FROM dbo.Tasks AS t
ORDER BY t.CreatedAt DESC;

-------------------------------------------------------------------------------
-- 3. Filtrar tareas por un valor dentro del JSON.
--    La columna calculada Tasks.Priority (= JSON_VALUE(AdditionalInfo, '$.priority'))
--    tiene el índice IX_Tasks_Priority, de modo que este filtro no recorre toda la tabla.
-------------------------------------------------------------------------------
SELECT t.Title, t.Status, t.Priority
FROM dbo.Tasks AS t
WHERE t.Priority = N'High';

-- Equivalente sin la columna calculada:
SELECT t.Title, t.Status
FROM dbo.Tasks AS t
WHERE JSON_VALUE(t.AdditionalInfo, '$.priority') = N'High';

-------------------------------------------------------------------------------
-- 4. JSON_QUERY: extraer fragmentos (arreglos u objetos) sin convertirlos a texto escalar.
-------------------------------------------------------------------------------
SELECT
    t.Title,
    JSON_QUERY(t.AdditionalInfo, '$.tags')     AS Tags,
    JSON_QUERY(t.AdditionalInfo, '$.metadata') AS Metadata
FROM dbo.Tasks AS t
WHERE t.AdditionalInfo IS NOT NULL;

-------------------------------------------------------------------------------
-- 5. OPENJSON: convertir el JSON en filas.
-------------------------------------------------------------------------------
-- 5.1 Tareas que tienen una etiqueta concreta.
SELECT t.Title, t.Status
FROM dbo.Tasks AS t
WHERE EXISTS (
    SELECT 1
    FROM OPENJSON(t.AdditionalInfo, '$.tags') AS tag
    WHERE tag.[value] = N'backend');

-- 5.2 Número de tareas por etiqueta.
SELECT tag.[value] AS Tag, COUNT(*) AS Tasks
FROM dbo.Tasks AS t
CROSS APPLY OPENJSON(t.AdditionalInfo, '$.tags') AS tag
GROUP BY tag.[value]
ORDER BY Tasks DESC, Tag;

-- 5.3 OPENJSON con esquema explícito (WITH) para leer el JSON como columnas tipadas.
SELECT t.Title, info.Priority, info.EstimatedCompletionDate, info.Tags
FROM dbo.Tasks AS t
CROSS APPLY OPENJSON(t.AdditionalInfo)
    WITH (
        Priority                NVARCHAR(10)  '$.priority',
        EstimatedCompletionDate DATE          '$.estimatedCompletionDate',
        Tags                    NVARCHAR(MAX) '$.tags' AS JSON
    ) AS info;

-------------------------------------------------------------------------------
-- 6. (Opcional) JSON_MODIFY: actualizar un campo concreto del JSON.
--    Se ejecuta en una transacción que se revierte para no alterar los datos de ejemplo.
-------------------------------------------------------------------------------
BEGIN TRANSACTION;

UPDATE dbo.Tasks
SET AdditionalInfo = JSON_MODIFY(
        JSON_MODIFY(AdditionalInfo, '$.priority', N'High'),   -- cambia un valor
        'append $.tags', N'urgent'),                           -- agrega un elemento al arreglo
    UpdatedAt = SYSUTCDATETIME()
WHERE Title = N'Documentar API en el README';

SELECT Title, AdditionalInfo, Priority
FROM dbo.Tasks
WHERE Title = N'Documentar API en el README';

ROLLBACK TRANSACTION;

-------------------------------------------------------------------------------
-- 7. Consulta del enunciado: tareas por usuario, filtradas por estado y ordenadas por fecha de creación.
--    La API la ejecuta mediante dbo.usp_GetTasks (GET /api/tasks?userId=...&status=...&sortDirection=desc).
-------------------------------------------------------------------------------
DECLARE @UserId UNIQUEIDENTIFIER = (SELECT Id FROM dbo.Users WHERE Email = N'ana.gomez@acme.com');

-- Devuelve dos conjuntos de resultados: el total de tareas filtradas y la página (sin @PageSize, todas).
EXEC dbo.usp_GetTasks @UserId = @UserId, @Status = N'Pending', @SortDescending = 1;

-- Paginado: segunda página de 10 tareas (GET /api/tasks?status=Pending&page=2&pageSize=10).
EXEC dbo.usp_GetTasks @Status = N'Pending', @PageNumber = 2, @PageSize = 10;

-- Combinando filtros relacionales y JSON (prioridad y etiqueta):
EXEC dbo.usp_GetTasks @Priority = N'Medium', @Tag = N'backend';

-------------------------------------------------------------------------------
-- 8. Logs de la API (Serilog): cada evento se guarda completo en JSON en dbo.Logs.LogEvent.
-------------------------------------------------------------------------------
-- Peticiones HTTP que terminaron con error (estado >= 400), con su duración.
SELECT TOP (50)
    CreatedAt,
    JSON_VALUE(LogEvent, '$.Properties.RequestMethod')                     AS Metodo,
    JSON_VALUE(LogEvent, '$.Properties.RequestPath')                       AS Ruta,
    TRY_CAST(JSON_VALUE(LogEvent, '$.Properties.StatusCode') AS int)       AS Estado,
    TRY_CAST(JSON_VALUE(LogEvent, '$.Properties.Elapsed') AS decimal(10, 1)) AS Ms
FROM dbo.Logs
WHERE TRY_CAST(JSON_VALUE(LogEvent, '$.Properties.StatusCode') AS int) >= 400
ORDER BY CreatedAt DESC;

-- Errores no controlados, con la excepción completa.
SELECT TOP (20) CreatedAt, Message, Exception
FROM dbo.Logs
WHERE Level IN (N'Error', N'Fatal')
ORDER BY CreatedAt DESC;

-- Rutas más lentas en promedio (OPENJSON con esquema explícito).
SELECT TOP (10) req.RequestPath, COUNT(*) AS Peticiones, AVG(req.Elapsed) AS MsPromedio
FROM dbo.Logs AS l
CROSS APPLY OPENJSON(l.LogEvent, '$.Properties')
    WITH (RequestPath NVARCHAR(400) '$.RequestPath', Elapsed FLOAT '$.Elapsed') AS req
WHERE req.RequestPath IS NOT NULL
GROUP BY req.RequestPath
ORDER BY MsPromedio DESC;
