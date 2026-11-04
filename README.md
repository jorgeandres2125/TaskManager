# TaskManager

Aplicación interna para registrar tareas, asignarlas a personas del equipo y seguir su estado en un tablero Kanban. La hice como prueba técnica fullstack con .NET 10, Angular 20 y SQL Server 2022.

> **Aviso:** la configuración apunta a un servidor de base de datos de demostración (`sql8020.site4now.net`) que **expira el 4 de enero de 2027**. A partir de esa fecha tendrás que usar tu propio SQL Server.

## Qué puedes hacer

- Crear, editar y reasignar tareas con título, responsable, prioridad, fecha estimada y etiquetas.
- Mover una tarea entre *Pendiente*, *En progreso* y *Completada* arrastrándola o con su menú. La API rechaza el paso directo de Pendiente a Completada.
- Filtrar el tablero por estado, prioridad, etiqueta o responsable.
- Gestionar usuarios con nombre, correo y foto (JPG, PNG o WebP de hasta 2 MB).

## Arquitectura

El repositorio tiene tres proyectos:

```
BackEnd_NET_1.1/        API REST en .NET 10
FrontEnd_Angular_1.1/   Interfaz en Angular 20
SQL_Server_1.1/         Scripts para recrear la base de datos
```

**Backend.** Sigue Clean Architecture en cuatro proyectos: `Domain` guarda las entidades y la regla de estados, `Application` los casos de uso y las validaciones (FluentValidation), `Infrastructure` el acceso a datos y `Api` los controllers. Las escrituras usan Entity Framework Core; las lecturas de tareas pasan por el procedimiento `dbo.usp_GetTasks` con Dapper. La información adicional de cada tarea vive en una columna JSON que SQL Server valida y consulta con sus funciones nativas. Serilog guarda los logs en la tabla `dbo.Logs`.

**Frontend.** Cada funcionalidad (`tasks`, `users`) separa dominio, acceso a la API, estado (signals y RxJS) y presentación. Usa Angular Material y Tailwind con un tema oscuro propio.

**Base de datos.** Cuatro tablas: `Users`, `Tasks`, `UserPhotos` (fotos en `VARBINARY(MAX)`) y `Logs`. Las migraciones de EF Core definen el esquema y `SQL_Server_1.1` lo exporta a scripts.

## Requisitos

- [.NET SDK 10](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org) 20.19, 22.12 o 24 (o posterior)
- Opcional: SQL Server 2022 y `sqlcmd`, si quieres tu propia base de datos

## Cómo correrlo en local

Abre dos terminales desde la raíz del repositorio.

**1. API** (queda en `http://localhost:15200`):

```bash
cd BackEnd_NET_1.1
dotnet run --project src/TaskManager.Api --launch-profile http
```

Con la API en marcha, Swagger te muestra todos los endpoints en `http://localhost:15200/swagger`.

**2. Interfaz** (queda en `http://localhost:4200`):

```bash
cd FrontEnd_Angular_1.1/TaskManager
npm ci
npm start
```

Abre `http://localhost:4200`. La base de demostración ya trae 200 usuarios y unas 1000 tareas para que pruebes desde el primer momento.

Mantén la interfaz en el puerto 4200: la API solo acepta peticiones del navegador desde ese origen (CORS). Si cambias el puerto o la dirección de la API, edita `Cors:AllowedOrigins` en los `appsettings` del backend y `apiBaseUrl` en `src/environments/` del frontend.

## Pruebas

```bash
cd BackEnd_NET_1.1 && dotnet test                                    # 77 pruebas
cd FrontEnd_Angular_1.1/TaskManager && npx ng test --watch=false     # 44 pruebas
```

## Usar tu propia base de datos

Necesitas SQL Server 2022 o posterior. Desde `SQL_Server_1.1` ejecuta el script que prefieras:

```bash
cd SQL_Server_1.1

# Copia exacta de la demostración (usuarios, tareas y fotos)
sqlcmd -S <servidor> -E -I -f 65001 -v DatabaseName=TaskManager -i recreate_database_demo.sql

# O datos de prueba generados
sqlcmd -S <servidor> -E -I -f 65001 -v DatabaseName=TaskManager -i recreate_database.sql
```

Después apunta la API a tu base sin tocar los archivos del repositorio:

```bash
cd BackEnd_NET_1.1
dotnet user-secrets set "ConnectionStrings:TaskManager" "Data Source=<servidor>;Initial Catalog=TaskManager;Integrated Security=True;TrustServerCertificate=True;Min Pool Size=5;Max Pool Size=10" --project src/TaskManager.Api
```

El SQL_Server_1.1/README.md explica cada script y cómo usarlos en un hosting donde ya exista la base.

## Decisiones técnicas

- **Clean Architecture en el backend.** Separé las reglas del negocio (dominio y casos de uso) de la tecnología (EF Core, Dapper, ASP.NET). Así pruebo las reglas sin base de datos y puedo cambiar el acceso a datos sin tocarlas.
- **EF Core para escribir y Dapper para leer.** EF Core gestiona las entidades y las migraciones. Para listar tareas uso el procedimiento `dbo.usp_GetTasks`, que filtra, ordena y pagina en una sola consulta y devuelve el total.
- **JSON nativo de SQL Server.** La columna `Tasks.AdditionalInfo` guarda prioridad, fecha estimada, etiquetas y metadatos. Una restricción CHECK la valida con `ISJSON(..., OBJECT)`, que exige SQL Server 2022. `JSON_VALUE` alimenta una columna calculada con índice para filtrar por prioridad, y `OPENJSON` permite buscar por etiqueta.
- **Transiciones de estado.** El enunciado solo prohíbe pasar de Pendiente a Completada. Yo también bloqueé Completada → Pendiente, para que una tarea reabierta pase por En progreso. La regla vive en el dominio (`TaskStatusTransitions`) y la API responde 409 si alguien la incumple; la interfaz lee de la API los cambios permitidos y no repite la regla.
- **Más rutas que las mínimas.** Además de las cinco del enunciado añadí `GET` por Id, edición de tareas y usuarios (`PUT /api/tasks/{id}` sirve para reasignar), fotos de usuario y paginación en los listados.
- **Errores uniformes.** La API devuelve todos los errores como ProblemDetails (RFC 9457) y el frontend los muestra en un aviso o junto al campo que falló.
- **Identificadores.** Las entidades usan GUID v7 como clave y un índice clustered sobre la fecha de creación. Las tareas tienen además un número correlativo (#24) que genera una secuencia de SQL Server.
- **Contraseña en los `appsettings`.** La subí a propósito para que la demostración funcione al clonar. El servidor es temporal y expira el 4 de enero de 2027.

## Funcionalidades pendientes

- Pruebas de integración de la API contra un SQL Server real. Hoy hay 77 pruebas unitarias en el backend y 44 en el frontend.
- Pruebas de extremo a extremo del frontend (Playwright o Cypress).
- Un endpoint para cambiar campos sueltos del JSON con `JSON_MODIFY`. El ejemplo existe en `SQL_Server_1.1/04_json_queries.sql`.
- Control de concurrencia: si dos personas editan la misma tarea, gana la última.
- Limpieza periódica de la tabla de logs.
- Autenticación y autorización, fuera del alcance del enunciado.

## Trazabilidad de requisitos

| Requisito | Dónde se cumple |
|---|---|
| Crear y listar usuarios con nombre y correo | `POST` y `GET /api/users`; pantalla Usuarios |
| Crear tareas con formulario reactivo | `POST /api/tasks`; ventana «Nueva tarea» |
| Asignar una tarea a un usuario y elegirlo de un listado | Campo `userId` obligatorio; selector con búsqueda; reasignación con `PUT /api/tasks/{id}` |
| Listar tareas y filtrar por estado | `GET /api/tasks?status=`; tablero Kanban y filtro «Estado» |
| Cambiar el estado de una tarea | `PUT /api/tasks/{id}/status`; arrastrar la tarjeta o usar su menú |
| Título obligatorio | Validador de la API y CHECK `CK_Tasks_Title_NotBlank` |
| Una tarea debe tener un usuario | Validador de la API y clave foránea `FK_Tasks_Users_UserId` |
| Prohibido pasar de Pending a Done | `TaskStatusTransitions` (respuesta 409); la interfaz no permite soltar la tarjeta en Completada |
| Obtener tareas por usuario | `GET /api/tasks?userId=`; panel del equipo |
| Ordenar por fecha de creación | `GET /api/tasks?sortDirection=`; `usp_GetTasks` ordena por `CreatedAt` |
| Claves primarias, foráneas e índices | `SQL_Server_1.1/01_schema.sql` |
| Script de creación de la base de datos | `SQL_Server_1.1/recreate_database.sql` |
| Funciones JSON nativas de SQL Server | `ISJSON`, `JSON_VALUE`, `OPENJSON` y `JSON_QUERY` en el esquema y en `usp_GetTasks`; ejemplos en `04_json_queries.sql` |
| Manejo de errores | ProblemDetails en la API; avisos y errores por campo en la interfaz |
