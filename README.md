# TaskManager

Aplicación interna para registrar tareas, asignarlas a personas del equipo y seguir su estado en un tablero Kanban. La hice como prueba técnica fullstack con .NET 10, Angular 20 y SQL Server 2022.

> **Aviso:** la configuración apunta a un servidor de base de datos de demostración (`sql8020.site4now.net`) que **expira el 4 de enero de 2027**. A partir de esa fecha tendrás que usar tu propio SQL Server.

## Qué puedes hacer

- Crear, editar y reasignar tareas con título, responsable, prioridad, fecha estimada y etiquetas.
- Mover una tarea entre *Pendiente*, *En progreso* y *Completada* arrastrándola o con su menú. La API rechaza el paso directo de Pendiente a Completada.
- Filtrar el tablero por prioridad, etiqueta o responsable.
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
cd FrontEnd_Angular_1.1/TaskManager && npx ng test --watch=false     # 43 pruebas
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
