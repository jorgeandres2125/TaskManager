using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManager.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Procedimiento dbo.usp_PatchTaskAdditionalInfo: cambia campos sueltos de Tasks.AdditionalInfo con JSON_MODIFY
    /// (prioridad, fecha estimada, etiquetas con "append $.tags", metadatos con rutas $.metadata."clave") sin
    /// reescribir el documento completo. Bloquea la fila durante la transacción y devuelve el resultado por su nombre
    /// (Updated, TaskNotFound, TooManyTags, TooManyMetadataEntries). Requiere SQL Server 2017 o posterior
    /// (rutas JSON en variables, STRING_AGG).
    /// </summary>
    public partial class AddPatchTaskAdditionalInfoProcedure : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.SqlInOwnBatch("""
                CREATE OR ALTER PROCEDURE dbo.usp_PatchTaskAdditionalInfo
                    @TaskId UNIQUEIDENTIFIER,
                    @Priority NVARCHAR(10) = NULL,
                    @ClearPriority BIT = 0,
                    @EstimatedCompletionDate DATE = NULL,
                    @ClearEstimatedCompletionDate BIT = 0,
                    @AddTags NVARCHAR(MAX) = NULL,
                    @RemoveTags NVARCHAR(MAX) = NULL,
                    @SetMetadata NVARCHAR(MAX) = NULL,
                    @RemoveMetadata NVARCHAR(MAX) = NULL,
                    @UpdatedAt DATETIME2,
                    @MaxTags INT,
                    @MaxMetadataEntries INT
                AS
                BEGIN
                    SET NOCOUNT ON;
                    SET XACT_ABORT ON;

                    -- Metadatos: una fila por clave; Value NULL significa quitarla.
                    DECLARE @MetadataChanges TABLE (Position INT IDENTITY PRIMARY KEY, MetadataKey NVARCHAR(4000) NOT NULL, Value NVARCHAR(MAX) NULL);
                    INSERT INTO @MetadataChanges (MetadataKey, Value)
                    SELECT value, NULL FROM OPENJSON(@RemoveMetadata);
                    INSERT INTO @MetadataChanges (MetadataKey, Value)
                    SELECT [key], value FROM OPENJSON(@SetMetadata);

                    -- La clave forma parte de la ruta JSON: solo se admiten letras, números, '-' y '_'
                    -- (el guion va primero en el patrón; al final, LIKE lo leería como un rango).
                    IF EXISTS (
                        SELECT 1
                        FROM @MetadataChanges
                        WHERE MetadataKey = N'' OR MetadataKey COLLATE Latin1_General_100_BIN2 LIKE N'%[^-A-Za-z0-9_]%')
                        THROW 50001, N'Clave de metadatos no válida.', 1;

                    BEGIN TRANSACTION;

                    -- UPDLOCK + HOLDLOCK: nadie más modifica el JSON de la tarea entre la lectura y la escritura.
                    DECLARE @Info NVARCHAR(MAX);
                    SELECT @Info = ISNULL(AdditionalInfo, N'{}')
                    FROM dbo.Tasks WITH (UPDLOCK, HOLDLOCK)
                    WHERE Id = @TaskId;

                    IF @@ROWCOUNT = 0
                    BEGIN
                        ROLLBACK TRANSACTION;
                        SELECT N'TaskNotFound' AS Result;
                        RETURN;
                    END;

                    -- En modo lax, JSON_MODIFY con NULL elimina la clave.
                    IF @ClearPriority = 1
                        SET @Info = JSON_MODIFY(@Info, N'$.priority', NULL);
                    ELSE IF @Priority IS NOT NULL
                        SET @Info = JSON_MODIFY(@Info, N'$.priority', @Priority);

                    IF @ClearEstimatedCompletionDate = 1
                        SET @Info = JSON_MODIFY(@Info, N'$.estimatedCompletionDate', NULL);
                    ELSE IF @EstimatedCompletionDate IS NOT NULL
                        SET @Info = JSON_MODIFY(@Info, N'$.estimatedCompletionDate', CONVERT(NVARCHAR(10), @EstimatedCompletionDate, 23));

                    IF JSON_QUERY(@Info, N'$.tags') IS NULL
                        SET @Info = JSON_MODIFY(@Info, N'$.tags', JSON_QUERY(N'[]'));

                    -- Quitar etiquetas: se reconstruye el array sin ellas, conservando el orden (sin distinguir mayúsculas).
                    IF @RemoveTags IS NOT NULL
                        SET @Info = JSON_MODIFY(@Info, N'$.tags', JSON_QUERY(N'[' + ISNULL((
                            SELECT STRING_AGG(N'"' + STRING_ESCAPE(current_tag.value, 'json') + N'"', N',')
                                   WITHIN GROUP (ORDER BY CAST(current_tag.[key] AS INT))
                            FROM OPENJSON(@Info, N'$.tags') AS current_tag
                            WHERE NOT EXISTS (
                                SELECT 1
                                FROM OPENJSON(@RemoveTags) AS removed
                                WHERE removed.value COLLATE Latin1_General_100_CI_AS = current_tag.value COLLATE Latin1_General_100_CI_AS)
                        ), N'') + N']'));

                    -- Añadir etiquetas: "append $.tags", solo si no están ya.
                    DECLARE @NewTags TABLE (Position INT PRIMARY KEY, Tag NVARCHAR(4000) NOT NULL);
                    INSERT INTO @NewTags (Position, Tag)
                    SELECT CAST([key] AS INT), value
                    FROM OPENJSON(@AddTags);

                    DECLARE @Tag NVARCHAR(4000);
                    DECLARE @Position INT = (SELECT MIN(Position) FROM @NewTags);
                    WHILE @Position IS NOT NULL
                    BEGIN
                        SELECT @Tag = Tag FROM @NewTags WHERE Position = @Position;

                        IF NOT EXISTS (
                            SELECT 1
                            FROM OPENJSON(@Info, N'$.tags') AS current_tag
                            WHERE current_tag.value COLLATE Latin1_General_100_CI_AS = @Tag COLLATE Latin1_General_100_CI_AS)
                            SET @Info = JSON_MODIFY(@Info, N'append $.tags', @Tag);

                        SET @Position = (SELECT MIN(Position) FROM @NewTags WHERE Position > @Position);
                    END;

                    IF (SELECT COUNT(*) FROM OPENJSON(@Info, N'$.tags')) > @MaxTags
                    BEGIN
                        ROLLBACK TRANSACTION;
                        SELECT N'TooManyTags' AS Result;
                        RETURN;
                    END;

                    IF JSON_QUERY(@Info, N'$.metadata') IS NULL
                        SET @Info = JSON_MODIFY(@Info, N'$.metadata', JSON_QUERY(N'{}'));

                    DECLARE @MetadataKey NVARCHAR(4000);
                    DECLARE @MetadataValue NVARCHAR(MAX);
                    SET @Position = (SELECT MIN(Position) FROM @MetadataChanges);
                    WHILE @Position IS NOT NULL
                    BEGIN
                        SELECT @MetadataKey = MetadataKey, @MetadataValue = Value
                        FROM @MetadataChanges
                        WHERE Position = @Position;

                        SET @Info = JSON_MODIFY(@Info, N'$.metadata."' + @MetadataKey + N'"', @MetadataValue);

                        SET @Position = (SELECT MIN(Position) FROM @MetadataChanges WHERE Position > @Position);
                    END;

                    IF (SELECT COUNT(*) FROM OPENJSON(@Info, N'$.metadata')) > @MaxMetadataEntries
                    BEGIN
                        ROLLBACK TRANSACTION;
                        SELECT N'TooManyMetadataEntries' AS Result;
                        RETURN;
                    END;

                    -- Igual que el dominio: sin ningún dato, la información adicional queda en NULL.
                    IF JSON_VALUE(@Info, N'$.priority') IS NULL
                       AND JSON_VALUE(@Info, N'$.estimatedCompletionDate') IS NULL
                       AND NOT EXISTS (SELECT 1 FROM OPENJSON(@Info, N'$.tags'))
                       AND NOT EXISTS (SELECT 1 FROM OPENJSON(@Info, N'$.metadata'))
                        SET @Info = NULL;

                    UPDATE dbo.Tasks
                    SET AdditionalInfo = @Info,
                        UpdatedAt = @UpdatedAt
                    WHERE Id = @TaskId;

                    COMMIT TRANSACTION;

                    SELECT N'Updated' AS Result;
                END;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.SqlInOwnBatch("DROP PROCEDURE IF EXISTS dbo.usp_PatchTaskAdditionalInfo;");
        }
    }
}
