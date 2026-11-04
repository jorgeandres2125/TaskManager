IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104043253_InitialCreate'
)
BEGIN
    CREATE TABLE [Users] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [Email] nvarchar(256) NOT NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_Users] PRIMARY KEY NONCLUSTERED ([Id]),
        CONSTRAINT [CK_Users_Email_NotBlank] CHECK (LEN(TRIM([Email])) > 0),
        CONSTRAINT [CK_Users_Name_NotBlank] CHECK (LEN(TRIM([Name])) > 0)
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104043253_InitialCreate'
)
BEGIN
    CREATE TABLE [Tasks] (
        [Id] uniqueidentifier NOT NULL,
        [Title] nvarchar(200) NOT NULL,
        [Status] nvarchar(20) NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [AdditionalInfo] nvarchar(max) NULL,
        [UpdatedAt] datetime2 NULL,
        [Priority] AS CAST(JSON_VALUE([AdditionalInfo], '$.priority') AS nvarchar(10)),
        [CreatedAt] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_Tasks] PRIMARY KEY NONCLUSTERED ([Id]),
        CONSTRAINT [CK_Tasks_AdditionalInfo_IsJson] CHECK ([AdditionalInfo] IS NULL OR ISJSON([AdditionalInfo], OBJECT) = 1),
        CONSTRAINT [CK_Tasks_AdditionalInfo_Priority] CHECK (JSON_VALUE([AdditionalInfo], '$.priority') IS NULL OR JSON_VALUE([AdditionalInfo], '$.priority') IN (N'Low', N'Medium', N'High')),
        CONSTRAINT [CK_Tasks_Status] CHECK ([Status] IN (N'Pending', N'InProgress', N'Done')),
        CONSTRAINT [CK_Tasks_Title_NotBlank] CHECK (LEN(TRIM([Title])) > 0),
        CONSTRAINT [FK_Tasks_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104043253_InitialCreate'
)
BEGIN
    CREATE CLUSTERED INDEX [CIX_Tasks_CreatedAt] ON [Tasks] ([CreatedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104043253_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Tasks_Priority] ON [Tasks] ([Priority]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104043253_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Tasks_Status_CreatedAt] ON [Tasks] ([Status], [CreatedAt] DESC);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104043253_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Tasks_UserId_Status_CreatedAt] ON [Tasks] ([UserId], [Status], [CreatedAt] DESC);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104043253_InitialCreate'
)
BEGIN
    CREATE CLUSTERED INDEX [CIX_Users_CreatedAt] ON [Users] ([CreatedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104043253_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [UX_Users_Email] ON [Users] ([Email]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104043253_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261104043253_InitialCreate', N'10.0.14');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104043309_AddGetTasksProcedure'
)
BEGIN
    EXEC(N'CREATE OR ALTER PROCEDURE dbo.usp_GetTasks
        @TaskId         UNIQUEIDENTIFIER = NULL,
        @UserId         UNIQUEIDENTIFIER = NULL,
        @Status         NVARCHAR(20)     = NULL,
        @Priority       NVARCHAR(10)     = NULL,
        @Tag            NVARCHAR(50)     = NULL,
        @SortDescending BIT              = 1
    AS
    BEGIN
        SET NOCOUNT ON;

        SELECT
            t.Id,
            t.Title,
            t.Status,
            t.UserId,
            u.Name AS UserName,
            t.CreatedAt,
            t.UpdatedAt,
            t.Priority,                                                                    -- columna calculada con JSON_VALUE
            TRY_CONVERT(date, JSON_VALUE(t.AdditionalInfo, ''$.estimatedCompletionDate'')) AS EstimatedCompletionDate,
            JSON_QUERY(t.AdditionalInfo, ''$.tags'')     AS Tags,                           -- arreglo JSON
            JSON_QUERY(t.AdditionalInfo, ''$.metadata'') AS Metadata                        -- objeto JSON
        FROM dbo.Tasks AS t
        INNER JOIN dbo.Users AS u ON u.Id = t.UserId
        WHERE (@TaskId   IS NULL OR t.Id = @TaskId)
          AND (@UserId   IS NULL OR t.UserId = @UserId)
          AND (@Status   IS NULL OR t.Status = @Status)
          AND (@Priority IS NULL OR t.Priority = @Priority)
          AND (@Tag      IS NULL OR EXISTS (
                  SELECT 1
                  FROM OPENJSON(t.AdditionalInfo, ''$.tags'') AS tag
                  WHERE tag.[value] = @Tag))
        ORDER BY
            CASE WHEN @SortDescending = 1 THEN t.CreatedAt END DESC,
            CASE WHEN @SortDescending = 0 THEN t.CreatedAt END ASC,
            t.Id
        -- Los filtros son opcionales: RECOMPILE genera un plan acorde a los parámetros recibidos
        -- y permite usar los índices por usuario, estado o prioridad.
        OPTION (RECOMPILE);
    END');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104043309_AddGetTasksProcedure'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261104043309_AddGetTasksProcedure', N'10.0.14');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104070351_AddPaginationToGetTasksProcedure'
)
BEGIN
    EXEC(N'CREATE OR ALTER PROCEDURE dbo.usp_GetTasks
        @TaskId         UNIQUEIDENTIFIER = NULL,
        @UserId         UNIQUEIDENTIFIER = NULL,
        @Status         NVARCHAR(20)     = NULL,
        @Priority       NVARCHAR(10)     = NULL,
        @Tag            NVARCHAR(50)     = NULL,
        @SortDescending BIT              = 1,
        @PageNumber     INT              = NULL,   -- desde 1; NULL equivale a 1
        @PageSize       INT              = NULL    -- NULL: sin paginar
    AS
    BEGIN
        SET NOCOUNT ON;

        -- 1. Claves de las tareas que cumplen los filtros. Se guardan en una tabla temporal para
        --    contar y paginar sin repetir el WHERE.
        SELECT t.Id, t.CreatedAt
        INTO #Filtered
        FROM dbo.Tasks AS t
        WHERE (@TaskId   IS NULL OR t.Id = @TaskId)
          AND (@UserId   IS NULL OR t.UserId = @UserId)
          AND (@Status   IS NULL OR t.Status = @Status)
          AND (@Priority IS NULL OR t.Priority = @Priority)                -- columna calculada con JSON_VALUE
          AND (@Tag      IS NULL OR EXISTS (
                  SELECT 1
                  FROM OPENJSON(t.AdditionalInfo, ''$.tags'') AS tag
                  WHERE tag.[value] = @Tag))
        -- Los filtros son opcionales: RECOMPILE genera un plan acorde a los parámetros recibidos
        -- y permite usar los índices por usuario, estado o prioridad.
        OPTION (RECOMPILE);

        -- 2. Total de tareas filtradas, para calcular las páginas.
        SELECT COUNT(*) AS TotalCount FROM #Filtered;

        -- 3. Página pedida, ordenada por fecha de creación.
        SELECT
            t.Id,
            t.Title,
            t.Status,
            t.UserId,
            u.Name AS UserName,
            t.CreatedAt,
            t.UpdatedAt,
            t.Priority,
            TRY_CONVERT(date, JSON_VALUE(t.AdditionalInfo, ''$.estimatedCompletionDate'')) AS EstimatedCompletionDate,
            JSON_QUERY(t.AdditionalInfo, ''$.tags'')     AS Tags,                           -- arreglo JSON
            JSON_QUERY(t.AdditionalInfo, ''$.metadata'') AS Metadata                        -- objeto JSON
        FROM #Filtered AS f
        INNER JOIN dbo.Tasks AS t ON t.Id = f.Id
        INNER JOIN dbo.Users AS u ON u.Id = t.UserId
        ORDER BY
            CASE WHEN @SortDescending = 1 THEN f.CreatedAt END DESC,
            CASE WHEN @SortDescending = 0 THEN f.CreatedAt END ASC,
            f.Id                                                                  -- desempate estable entre páginas
        OFFSET ISNULL((ISNULL(@PageNumber, 1) - 1) * @PageSize, 0) ROWS
        FETCH NEXT ISNULL(@PageSize, 2147483647) ROWS ONLY;
    END');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104070351_AddPaginationToGetTasksProcedure'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261104070351_AddPaginationToGetTasksProcedure', N'10.0.14');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104071044_AddLogsTable'
)
BEGIN
    EXEC(N'CREATE TABLE dbo.Logs
    (
        Id        UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Logs_Id DEFAULT NEWSEQUENTIALID(),
        CreatedAt DATETIME2        NOT NULL CONSTRAINT DF_Logs_CreatedAt DEFAULT SYSUTCDATETIME(),
        Level     NVARCHAR(16)     NOT NULL,
        Message   NVARCHAR(MAX)    NULL,
        Exception NVARCHAR(MAX)    NULL,
        LogEvent  NVARCHAR(MAX)    NULL,
        CONSTRAINT PK_Logs PRIMARY KEY NONCLUSTERED (Id),
        CONSTRAINT CK_Logs_LogEvent_IsJson CHECK (LogEvent IS NULL OR ISJSON(LogEvent, OBJECT) = 1)
    );');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104071044_AddLogsTable'
)
BEGIN
    EXEC(N'CREATE CLUSTERED INDEX CIX_Logs_CreatedAt ON dbo.Logs (CreatedAt);');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104071044_AddLogsTable'
)
BEGIN
    EXEC(N'CREATE NONCLUSTERED INDEX IX_Logs_Level_CreatedAt ON dbo.Logs (Level, CreatedAt DESC);');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104071044_AddLogsTable'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261104071044_AddLogsTable', N'10.0.14');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104080804_AddTaskNumber'
)
BEGIN
    CREATE SEQUENCE [dbo].[TaskNumberSequence] START WITH 1 INCREMENT BY 1 NO CYCLE;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104080804_AddTaskNumber'
)
BEGIN
    EXEC(N'ALTER TABLE dbo.Tasks ADD Number BIGINT NULL;');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104080804_AddTaskNumber'
)
BEGIN
    EXEC(N'DECLARE @Numbers TABLE (Id UNIQUEIDENTIFIER PRIMARY KEY, Number BIGINT NOT NULL);

    -- Números que ya aparecen en el título: "... (#24)".
    INSERT INTO @Numbers (Id, Number)
    SELECT t.Id,
           TRY_CAST(SUBSTRING(t.Title, CHARINDEX(N'' (#'', t.Title) + 3,
                              LEN(t.Title) - CHARINDEX(N'' (#'', t.Title) - 3) AS BIGINT)
    FROM dbo.Tasks AS t
    WHERE t.Title LIKE N''% (#[0-9]%)''
      AND TRY_CAST(SUBSTRING(t.Title, CHARINDEX(N'' (#'', t.Title) + 3,
                             LEN(t.Title) - CHARINDEX(N'' (#'', t.Title) - 3) AS BIGINT) IS NOT NULL;

    -- Si hubiera números repetidos en los títulos, no se usan: todas se numeran por fecha.
    IF EXISTS (SELECT Number FROM @Numbers GROUP BY Number HAVING COUNT(*) > 1)
        DELETE FROM @Numbers;

    -- El resto, por fecha de creación; después del mayor número si hubiera choques.
    DECLARE @Offset BIGINT = 0;
    IF EXISTS (
        SELECT 1
        FROM (SELECT ROW_NUMBER() OVER (ORDER BY CreatedAt, Id) AS Number
              FROM dbo.Tasks
              WHERE Id NOT IN (SELECT Id FROM @Numbers)) AS pending
        WHERE pending.Number IN (SELECT Number FROM @Numbers))
        SELECT @Offset = MAX(Number) FROM @Numbers;

    INSERT INTO @Numbers (Id, Number)
    SELECT Id, ROW_NUMBER() OVER (ORDER BY CreatedAt, Id) + @Offset
    FROM dbo.Tasks
    WHERE Id NOT IN (SELECT Id FROM @Numbers);

    UPDATE t
    SET t.Number = n.Number,
        t.Title = CASE WHEN t.Title LIKE N''% (#[0-9]%)''
                       THEN LEFT(t.Title, CHARINDEX(N'' (#'', t.Title) - 1)
                       ELSE t.Title END
    FROM dbo.Tasks AS t
    INNER JOIN @Numbers AS n ON n.Id = t.Id;');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104080804_AddTaskNumber'
)
BEGIN
    EXEC(N'ALTER TABLE dbo.Tasks ALTER COLUMN Number BIGINT NOT NULL;');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104080804_AddTaskNumber'
)
BEGIN
    EXEC(N'ALTER TABLE dbo.Tasks ADD CONSTRAINT DF_Tasks_Number DEFAULT (NEXT VALUE FOR dbo.TaskNumberSequence) FOR Number;');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104080804_AddTaskNumber'
)
BEGIN
    EXEC(N'DECLARE @Next BIGINT = (SELECT ISNULL(MAX(Number), 0) + 1 FROM dbo.Tasks);
    DECLARE @Sql NVARCHAR(200) = N''ALTER SEQUENCE dbo.TaskNumberSequence RESTART WITH '' + CAST(@Next AS NVARCHAR(20)) + N'';'';
    EXEC sys.sp_executesql @Sql;');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104080804_AddTaskNumber'
)
BEGIN
    CREATE UNIQUE INDEX [UX_Tasks_Number] ON [Tasks] ([Number]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104080804_AddTaskNumber'
)
BEGIN
    EXEC(N'CREATE OR ALTER PROCEDURE dbo.usp_GetTasks
        @TaskId         UNIQUEIDENTIFIER = NULL,
        @UserId         UNIQUEIDENTIFIER = NULL,
        @Status         NVARCHAR(20)     = NULL,
        @Priority       NVARCHAR(10)     = NULL,
        @Tag            NVARCHAR(50)     = NULL,
        @SortDescending BIT              = 1,
        @PageNumber     INT              = NULL,   -- desde 1; NULL equivale a 1
        @PageSize       INT              = NULL    -- NULL: sin paginar
    AS
    BEGIN
        SET NOCOUNT ON;
        
        -- 1. Claves de las tareas que cumplen los filtros. Se guardan en una tabla temporal para
        --    contar y paginar sin repetir el WHERE.
        SELECT t.Id, t.CreatedAt
        INTO #Filtered
        FROM dbo.Tasks AS t
        WHERE (@TaskId   IS NULL OR t.Id = @TaskId)
          AND (@UserId   IS NULL OR t.UserId = @UserId)
          AND (@Status   IS NULL OR t.Status = @Status)
          AND (@Priority IS NULL OR t.Priority = @Priority)                -- columna calculada con JSON_VALUE
          AND (@Tag      IS NULL OR EXISTS (
                  SELECT 1
                  FROM OPENJSON(t.AdditionalInfo, ''$.tags'') AS tag
                  WHERE tag.[value] = @Tag))
        -- Los filtros son opcionales: RECOMPILE genera un plan acorde a los parámetros recibidos
        -- y permite usar los índices por usuario, estado o prioridad.
        OPTION (RECOMPILE);

        -- 2. Total de tareas filtradas, para calcular las páginas.
        SELECT COUNT(*) AS TotalCount FROM #Filtered;

        -- 3. Página pedida, ordenada por fecha de creación.
        SELECT
            t.Id,
            t.Number,
            t.Title,
            t.Status,
            t.UserId,
            u.Name AS UserName,
            t.CreatedAt,
            t.UpdatedAt,
            t.Priority,
            TRY_CONVERT(date, JSON_VALUE(t.AdditionalInfo, ''$.estimatedCompletionDate'')) AS EstimatedCompletionDate,
            JSON_QUERY(t.AdditionalInfo, ''$.tags'')     AS Tags,                           -- arreglo JSON
            JSON_QUERY(t.AdditionalInfo, ''$.metadata'') AS Metadata                        -- objeto JSON
        FROM #Filtered AS f
        INNER JOIN dbo.Tasks AS t ON t.Id = f.Id
        INNER JOIN dbo.Users AS u ON u.Id = t.UserId
        ORDER BY
            CASE WHEN @SortDescending = 1 THEN f.CreatedAt END DESC,
            CASE WHEN @SortDescending = 0 THEN f.CreatedAt END ASC,
            f.Id                                                                  -- desempate estable entre páginas
        OFFSET ISNULL((ISNULL(@PageNumber, 1) - 1) * @PageSize, 0) ROWS
        FETCH NEXT ISNULL(@PageSize, 2147483647) ROWS ONLY;
    END');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104080804_AddTaskNumber'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261104080804_AddTaskNumber', N'10.0.14');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104123905_PartialTagFilterInGetTasks'
)
BEGIN
    EXEC(N'CREATE OR ALTER PROCEDURE dbo.usp_GetTasks
        @TaskId         UNIQUEIDENTIFIER = NULL,
        @UserId         UNIQUEIDENTIFIER = NULL,
        @Status         NVARCHAR(20)     = NULL,
        @Priority       NVARCHAR(10)     = NULL,
        @Tag            NVARCHAR(50)     = NULL,
        @SortDescending BIT              = 1,
        @PageNumber     INT              = NULL,   -- desde 1; NULL equivale a 1
        @PageSize       INT              = NULL    -- NULL: sin paginar
    AS
    BEGIN
        SET NOCOUNT ON;
        
        -- Búsqueda parcial de etiqueta: "back" encuentra "backend". Los comodines se escapan para buscar el texto literal.
        DECLARE @TagPattern NVARCHAR(210) = N''%'' + REPLACE(REPLACE(REPLACE(REPLACE(
            @Tag, N''!'', N''!!''), N''%'', N''!%''), N''_'', N''!_''), N''['', N''!['') + N''%'';

        -- 1. Claves de las tareas que cumplen los filtros. Se guardan en una tabla temporal para
        --    contar y paginar sin repetir el WHERE.
        SELECT t.Id, t.CreatedAt
        INTO #Filtered
        FROM dbo.Tasks AS t
        WHERE (@TaskId   IS NULL OR t.Id = @TaskId)
          AND (@UserId   IS NULL OR t.UserId = @UserId)
          AND (@Status   IS NULL OR t.Status = @Status)
          AND (@Priority IS NULL OR t.Priority = @Priority)                -- columna calculada con JSON_VALUE
          AND (@Tag      IS NULL OR EXISTS (
                  SELECT 1
                  FROM OPENJSON(t.AdditionalInfo, ''$.tags'') AS tag
                  WHERE tag.[value] LIKE @TagPattern ESCAPE N''!''))
        -- Los filtros son opcionales: RECOMPILE genera un plan acorde a los parámetros recibidos
        -- y permite usar los índices por usuario, estado o prioridad.
        OPTION (RECOMPILE);

        -- 2. Total de tareas filtradas, para calcular las páginas.
        SELECT COUNT(*) AS TotalCount FROM #Filtered;

        -- 3. Página pedida, ordenada por fecha de creación.
        SELECT
            t.Id,
            t.Number,
            t.Title,
            t.Status,
            t.UserId,
            u.Name AS UserName,
            t.CreatedAt,
            t.UpdatedAt,
            t.Priority,
            TRY_CONVERT(date, JSON_VALUE(t.AdditionalInfo, ''$.estimatedCompletionDate'')) AS EstimatedCompletionDate,
            JSON_QUERY(t.AdditionalInfo, ''$.tags'')     AS Tags,                           -- arreglo JSON
            JSON_QUERY(t.AdditionalInfo, ''$.metadata'') AS Metadata                        -- objeto JSON
        FROM #Filtered AS f
        INNER JOIN dbo.Tasks AS t ON t.Id = f.Id
        INNER JOIN dbo.Users AS u ON u.Id = t.UserId
        ORDER BY
            CASE WHEN @SortDescending = 1 THEN f.CreatedAt END DESC,
            CASE WHEN @SortDescending = 0 THEN f.CreatedAt END ASC,
            f.Id                                                                  -- desempate estable entre páginas
        OFFSET ISNULL((ISNULL(@PageNumber, 1) - 1) * @PageSize, 0) ROWS
        FETCH NEXT ISNULL(@PageSize, 2147483647) ROWS ONLY;
    END');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104123905_PartialTagFilterInGetTasks'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261104123905_PartialTagFilterInGetTasks', N'10.0.14');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104130544_AddUserPhotos'
)
BEGIN
    CREATE TABLE [UserPhotos] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [FileName] nvarchar(255) NOT NULL,
        [Extension] nvarchar(10) NOT NULL,
        [ContentType] nvarchar(50) NOT NULL,
        [SizeBytes] int NOT NULL,
        [Content] varbinary(max) NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_UserPhotos] PRIMARY KEY NONCLUSTERED ([Id]),
        CONSTRAINT [CK_UserPhotos_Extension_Format] CHECK ([Extension] LIKE N'.%'),
        CONSTRAINT [CK_UserPhotos_FileName_NotBlank] CHECK (LEN(TRIM([FileName])) > 0),
        CONSTRAINT [CK_UserPhotos_SizeBytes] CHECK ([SizeBytes] > 0 AND [SizeBytes] <= 2097152),
        CONSTRAINT [CK_UserPhotos_SizeBytes_MatchesContent] CHECK ([SizeBytes] = DATALENGTH([Content])),
        CONSTRAINT [FK_UserPhotos_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104130544_AddUserPhotos'
)
BEGIN
    CREATE CLUSTERED INDEX [CIX_UserPhotos_CreatedAt] ON [UserPhotos] ([CreatedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104130544_AddUserPhotos'
)
BEGIN
    CREATE UNIQUE INDEX [UX_UserPhotos_UserId] ON [UserPhotos] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261104130544_AddUserPhotos'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261104130544_AddUserPhotos', N'10.0.14');
END;

COMMIT;
GO

