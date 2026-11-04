# TaskManager · Base de datos (SQL Server 2022)

Scripts para recrear la base de datos completa: esquema, procedimiento almacenado, historial de migraciones de EF Core y datos de prueba.

**Requisito:** SQL Server 2022 (16.x) o posterior. El esquema usa `ISJSON(..., OBJECT)` en restricciones CHECK, y los datos de prueba usan `JSON_OBJECT` y `JSON_ARRAY`. `00_create_database.sql` detiene la ejecución si la versión es anterior.

## Scripts

| Orden | Script | Qué hace |
|---|---|---|
| 0 | `00_create_database.sql` | Crea la base (si no existe) y fija el nivel de compatibilidad 160. |
| 1 | `01_schema.sql` | Tablas `Users`, `Tasks`, `UserPhotos` y `Logs`; secuencia `TaskNumberSequence`; claves, índices y restricciones (incluida la validación de JSON); procedimiento `dbo.usp_GetTasks`; y la tabla `__EFMigrationsHistory`. **Se genera desde las migraciones de EF Core, no se edita a mano.** |
| 2 | `02_seed.sql` | Datos de ejemplo básicos: 3 usuarios y 5 tareas. |
| 3 | `03_seed_bulk.sql` | Datos de prueba hasta 200 usuarios y 1000 tareas, con la información adicional generada con `JSON_OBJECT` y `JSON_ARRAY`. |
| 5 | `05_demo_data.sql` | **Datos exactos de la base de demostración**: los 200 usuarios, las 1005 tareas (con su número, fechas e información adicional) y las 200 fotos. Alternativa a 02 + 03. |
| — | `04_json_queries.sql` | Consultas de ejemplo con las funciones JSON de SQL Server (`ISJSON`, `JSON_VALUE`, `JSON_QUERY`, `OPENJSON`, `JSON_MODIFY`…). No forma parte de la recreación. |
| — | `recreate_database.sql` | Script maestro con datos de prueba generados: ejecuta 00, 01, 02 y 03. |
| — | `recreate_database_demo.sql` | Script maestro con los datos exactos de la demostración: ejecuta 00, 01 y 05. |
| — | `drop_database.sql` | Elimina la base (irreversible). Exige confirmación. |

Todos los scripts de la recreación son **idempotentes**: se pueden ejecutar de nuevo sin duplicar datos ni fallar. `01_schema.sql` registra cada migración en `__EFMigrationsHistory`, así que una base creada con estos scripts queda al día para `dotnet ef database update`.

## Recrear la base de datos

Hay dos formas, según los datos que quiera:

- **`recreate_database_demo.sql`**: la base de demostración **tal cual** (mismos usuarios, tareas, números, fechas y fotos).
- **`recreate_database.sql`**: datos de prueba **generados** (200 usuarios y 1000 tareas del mismo tipo, pero distintos en cada recreación).

Con `sqlcmd`, **desde esta carpeta** (`:r` usa rutas relativas al directorio actual) y conectado a `master`:

```bash
cd source_code/SQL_Server_1.1

# Datos exactos de la demostración
sqlcmd -S <servidor> -E -I -f 65001 -v DatabaseName=TaskManager -i recreate_database_demo.sql

# Datos de prueba generados (autenticación de Windows)
sqlcmd -S <servidor> -E -I -f 65001 -v DatabaseName=TaskManager -i recreate_database.sql

# Usuario de SQL Server
sqlcmd -S <servidor> -U <usuario> -P <clave> -I -f 65001 -v DatabaseName=TaskManager -i recreate_database.sql
```

- `-I` activa `QUOTED_IDENTIFIER`. Es obligatorio: SQL Server lo exige para escribir en tablas con índices sobre columnas calculadas (`Tasks.Priority`).
- `-f 65001` lee los scripts como UTF-8, para conservar tildes y eñes en los datos.

Para empezar de cero, elimine antes la base. La confirmación es el mismo nombre de la base:

```bash
sqlcmd -S <servidor> -E -I -f 65001 -v DatabaseName=TaskManager ConfirmDrop=TaskManager -i drop_database.sql
```

**Hosting compartido**, donde la base la crea el panel del proveedor y no hay permiso `CREATE DATABASE`: ejecute los scripts uno a uno sobre la base existente, sin `00_create_database.sql`:

```bash
sqlcmd -S <servidor> -d <base> -U <usuario> -P <clave> -I -f 65001 -i 01_schema.sql
sqlcmd -S <servidor> -d <base> -U <usuario> -P <clave> -I -f 65001 -i 05_demo_data.sql
# o, en lugar de 05, los datos generados: 02_seed.sql y después 03_seed_bulk.sql
```

Después, apunte la API a esa base (`ConnectionStrings:TaskManager`, ver el README principal).

## Datos de la demostración (`05_demo_data.sql`)

- Contiene los datos de la base de demostración exportados con `tools/export_demo_data.py`: usuarios, tareas y fotos (las imágenes como literales binarios `0x…`, unos 2,2 MB; el script pesa unos 4,9 MB). Las fotos incluyen imágenes subidas por usuarios de la demostración: tenga en cuenta sus derechos antes de publicar el repositorio.
- Se aplica sobre el esquema vacío. Es idempotente (inserta solo los Id que faltan) y al final reinicia la secuencia de números de tarea en `MAX(Number) + 1`.
- Si la base de demostración cambia, regenérelo (desde esta carpeta; necesita Python 3 y `sqlcmd`, y usa la cadena de conexión de `appsettings.Development.json` de la API):

  ```bash
  python tools/export_demo_data.py
  ```

## Qué no incluyen

- **Logs:** la tabla `dbo.Logs` se crea vacía; la llena la API (Serilog).
- **Fotos con los datos generados:** `02_seed.sql` y `03_seed_bulk.sql` no traen imágenes; esos usuarios muestran la imagen por defecto del frontend. Las fotos se suben desde la aplicación (`PUT /api/users/{id}/photo`).

## Mantenimiento

- **Cambios de esquema:** se hacen con una migración de EF Core en el backend. Después se regenera `01_schema.sql` (desde `source_code/BackEnd_NET_1.1`):

  ```bash
  dotnet ef migrations script --idempotent --project src/TaskManager.Infrastructure --startup-project src/TaskManager.Api -o ../SQL_Server_1.1/01_schema.sql
  ```

- **SQL propio en una migración:** use `migrationBuilder.SqlInOwnBatch(...)` en lugar de `migrationBuilder.Sql(...)`. En el script idempotente EF mete cada operación en un `IF … BEGIN … END`, y sin un lote propio un `CREATE PROCEDURE` o el uso de una columna recién añadida fallan en una base vacía.
- **Secuencia de números de tarea:** si borra tareas de prueba, o tras ejecutar `04_json_queries.sql` (su `INSERT` rechazado consume un número), reiníciela en `MAX(Number) + 1`:

  ```sql
  DECLARE @Next BIGINT = (SELECT ISNULL(MAX(Number), 0) + 1 FROM dbo.Tasks);
  DECLARE @Sql NVARCHAR(200) = N'ALTER SEQUENCE dbo.TaskNumberSequence RESTART WITH ' + CAST(@Next AS NVARCHAR(20));
  EXEC sys.sp_executesql @Sql;
  ```

## Verificación

- **Desde cero**, en una base vacía: recreación completa, segunda ejecución sin cambios (idempotencia), procedimiento `usp_GetTasks` funcionando, EF Core sin migraciones pendientes y `drop_database.sql` (solo borra con la confirmación correcta). En esta máquina el LocalDB de SQL Server 2025 está dañado, así que se usó LocalDB 2019 con una copia temporal que solo sustituye las tres funciones de 2022.
- **`recreate_database_demo.sql` desde cero**, ejecutado dos veces: la huella SHA-256 de todos los usuarios, tareas y fotos (y el valor de la secuencia) es idéntica a la de la base de demostración original.
- **Sobre SQL Server 2022 real** (la base de demostración, que ya estaba completa): los scripts originales, sin cambios, se ejecutan sin errores y no modifican nada.
