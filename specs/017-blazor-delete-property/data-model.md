# Modelo de datos: eliminación lógica de propiedades

**Iniciativa**: `017-blazor-delete-property`  
**Plan relacionado**: [plan.md](./plan.md)  
**Tarea de migración**: T020 en [tasks.md](./tasks.md)

## Entidad Property

La entidad existente `Property` se conserva. La eliminación lógica añade un indicador de estado sin borrar la fila ni sus demás datos.

| Campo | Tipo lógico | Nulable | Valor predeterminado | Propósito |
|---|---|---:|---:|---|
| `IsDeleted` | Booleano | No | `false` | `false` identifica una propiedad activa; `true` identifica una propiedad retirada del catálogo. |

No se añade una fecha de borrado: el alcance necesita distinguir registros activos y retirados, pero no requiere conservar cuándo ocurrió la retirada. `UpdatedAt` se actualiza al realizar la transición.

## Ciclo de vida y reglas de consulta

- **Activa**: `IsDeleted = false`; aparece en las consultas normales y puede actualizarse según las reglas existentes.
- **Retirada**: `IsDeleted = true`; el registro y sus campos se conservan, pero no aparece en listado, detalle ni consultas normales, y no puede modificarse mediante los flujos normales.
- La transición permitida en esta iniciativa es `Activa -> Retirada`. No se ofrece restauración.
- Una solicitud repetida para una propiedad retirada la trata como inexistente y devuelve `404 Not Found`.
- El seeder puede omitir el filtro global para detectar un identificador ya retirado, pero no debe modificarlo, reinsertarlo ni reactivarlo.
- `imageUrl` y el recurso asociado no cambian como parte de esta transición.

## Persistencia y migración

La migración EF Core `20261123195817_SoftDeleteProperties` agrega `IsDeleted` a `properties` como columna booleana obligatoria con `DEFAULT FALSE`. El default asigna `false` a las filas existentes, por lo que todas permanecen activas al desplegar el cambio. La migración no modifica otras columnas ni elimina datos.

Archivos de migración:

- `app/backend/src/RealtorApi/Infrastructure/Persistence/Migrations/20261123195817_SoftDeleteProperties.cs`
- `app/backend/src/RealtorApi/Infrastructure/Persistence/Migrations/20261123195817_SoftDeleteProperties.Designer.cs`
- `app/backend/src/RealtorApi/Infrastructure/Persistence/Migrations/AppDbContextModelSnapshot.cs`

La configuración del modelo debe mantener la columna no nula y el default `false`, además de aplicar el filtro global `IsDeleted == false` a las consultas ordinarias. La eliminación actualiza condicionalmente solo una fila activa a retirada; el número de filas afectadas preserva el resultado `200`/`404`, incluso ante solicitudes concurrentes.

La migración debe aplicarse a la base de datos de cada entorno antes de desplegar una versión de la aplicación que lea o escriba `IsDeleted`. En SQLite, las pruebas usan `EnsureCreated` sobre el modelo actual; las operaciones y el default de la migración se verifican de forma específica en las pruebas de esquema.

## Riesgo de reversión

La operación `Down` elimina la columna `IsDeleted`. Si se revierte, se pierde la distinción entre registros activos y retirados y una versión anterior que no aplique filtro podría volver a mostrar las propiedades retiradas. La reversión debe coordinarse con el despliegue de la aplicación y con una decisión explícita sobre conservar/restaurar esa distinción.
