# Plan de implementación: 017-blazor-delete-property

**Branch**: `feat/017-blazor-delete-property` | **Date**: 2026-11-24 | **Spec**: `specs/017-blazor-delete-property/spec.md`
**Input**: Especificación funcional desde `specs/017-blazor-delete-property/spec.md`

## Artefactos de la iniciativa

- `spec.md`: alcance funcional aprobado.
- `plan.md`: decisiones y plan técnico.
- `data-model.md`: obligatorio para documentar el modelo persistente y la migración de esta iniciativa.
- `tasks.md`: trabajo trazado, incluida la dependencia de la migración respecto de `data-model.md`.
- `quickstart.md`: evidencia y pasos de validación requeridos para el cierre.

## Resumen

Implementar en `RealtorWeb` el flujo de eliminación. Añadir un slice vertical público para retirar lógicamente una propiedad y una acción confirmable desde cada tarjeta del listado Blazor. La API tipada existente en Refit será el único mecanismo de comunicación del frontend. Las consultas y las operaciones normales solo incluirán propiedades activas.

## Contexto Técnico

**Language/Version**: C# con .NET 11 (`net11.0`, SDK `11.0.100-preview.5.26302.115`)

**Primary Dependencies**: Blazor Web App (Razor Components), Refit + `Refit.HttpClientFactory`, Lucide.Blazor

**Storage**: N/A en frontend (consume API backend; sin cambios de persistencia en esta feature)

**Testing**: xUnit (`app/frontend/test/RealtorWeb/RealtorWebTests.csproj`) y validación manual guiada

**Target Platform**: ASP.NET Core Interactive Server + navegadores web

**Project Type**: Aplicación web (frontend Blazor con integración backend existente)

**Performance Goals**: En la eliminación de una propiedad, debe existir una validación manual del flujo de borrado: debiendo mostrarse un mensaje para que el usuario confirme su borrado.

**Constraints**:
- Mantener consistencia visual con `wwwroot/app.css` y componentes existentes
- Reutilizar patrones de manejo de error y servicios HTTP existentes

**Scale/Scope**: Una nueva opción en el listado de inmuebles + componente de feature asociados + extensión del cliente API y mostrado de los botones de Confirmar o Cancelar el borrado

## Constitution Check

*GATE: Debe aprobar antes de Fase 0 (investigación). Revalidar tras Fase 1 (diseño).*

### Integración y errores

- El endpoint usa Minimal API y `ISlice`, con validación FluentValidation y `ProblemDetails`, siguiendo los slices de propiedades existentes.
- Se mantiene el parámetro como texto en la ruta para validar el UUID y responder `400` en lugar de dejar que el enrutamiento produzca `404`.
- `IPropertiesApi` incorpora la operación Refit DELETE.
- La tarjeta controla un estado de envío para impedir duplicados. Solo notifica al listado después de una respuesta exitosa; los errores HTTP o de conexión conservan la tarjeta.
- El listado actualiza su página y el mensaje global tras la eliminación, ajustando la página cuando se elimina su último elemento.

### Pre-Fase 0

- **Principio I - Solución única y compartida**: PASS
  - La iniciativa extiende el frontend existente sin crear soluciones paralelas.
- **Principio II - Spec-Driven Development**: PASS
  - Existe `spec.md` en `specs/017-blazor-delete-property/` y el plan se deriva únicamente de esa spec.
- **Principio III - Arquitectura canónica**: PASS
  - Cambios acotados a Blazor Web App y consumo de Minimal API existente.
- **Principio IV - Stack obligatorio**: PASS
  - Se mantiene .NET 11, Blazor, Refit y estilos propios.
- **Principio V - Calidad de dominio y contratos**: PASS
  - El plan respeta contrato OpenAPI como fuente de verdad para integración.
- **Principio VI - Idioma y documentación**: PASS
  - Artefactos en español.
- **Principio IX - Calidad/verificabilidad de especificaciones**: PASS
  - Requisitos y criterios de éxito observables en la spec.

### Post-Fase 1

- **Revalidación**: PASS
  - `research.md`, `data-model.md`, `contracts/` y `quickstart.md` mantienen separación funcional/técnica y no introducen alcance no aprobado.


## Decisiones técnicas

### Estrategia de eliminación

Se usará eliminación lógica. El registro y sus datos se conservarán, mientras que las consultas y mutaciones normales tratarán la propiedad retirada como inexistente. La decisión de persistencia, el campo de estado, las reglas de filtrado y la migración se especifican en [data-model.md](./data-model.md).

### Imagen asociada

Se elimina únicamente el registro de la propiedad. El recurso señalado por `imageUrl` queda fuera del alcance y no se borra ni modifica. El archivo puede ser compartido o estar gestionado fuera del registro, y eliminarlo aquí podría afectar a otros usos.

### Confirmación y ubicación de la acción

La confirmación se integra en la tarjeta del listado mediante controles visibles de Confirmar y Cancelar. La acción no estará en la página de detalle. Esta solución identifica el elemento afectado, no depende de JavaScript interop y permite cancelar sin generar tráfico.

## Estructura afectada

```text
app/
├── backend/
│   ├── src/RealtorApi/
│   │   ├── Domain/Properties/Property.cs
│   │   ├── Features/Properties/
│   │   │   ├── DeleteProperty/
│   │   │   ├── GetPropertyById/GetPropertyByIdHandler.cs
│   │   │   ├── UpdateProperty/UpdatePropertyHandler.cs
│   │   │   └── UpdateStatus/UpdateStatusHandler.cs
│   │   ├── Infrastructure/Persistence/
│   │   │   ├── Configurations/PropertyConfiguration.cs
│   │   │   ├── DatabaseSeeder.cs
│   │   │   └── Migrations/20261123195817_SoftDeleteProperties
│   │   └── wwwroot/openapi/v1.json
│   └── tests/RealtorApiTests/Infrastructure/
│       ├── Endpoints/DeletePropertyEndpointTests.cs
│       └── Persistence/
└── frontend/src/RealtorWeb/
    ├── Features/Properties/List/
    ├── Services/Api/Properties/IPropertiesApi.cs
    └── wwwroot/app.css
```

Los artefactos obligatorios son `spec.md`, `plan.md` y `tasks.md`. Como esta iniciativa modifica el modelo persistido y requiere una migración, [data-model.md](./data-model.md) es obligatorio y se usa como referencia técnica para crear y validar esa migración.

## Validación

- Pruebas de endpoint para `200 OK`, `400`, `404`, persistencia de `IsDeleted`, exclusión de consultas normales, integridad del catálogo y solicitudes concurrentes.
- Pruebas de modelo/migración para el valor predeterminado `false`, el filtro global y su aplicación al esquema.
- Pruebas que confirman que el endpoint de actualización, el cambio de estado y el seeder no reactivan ni modifican propiedades retiradas.
- Pruebas existentes del backend y frontend.
- Compilación de backend y frontend.
- Validación de consistencia entre las rutas funcionales publicadas en OpenAPI y el endpoint runtime.
- Revisión manual guiada de confirmar, cancelar, feedback de éxito, error y bloqueo de solicitudes duplicadas.

## Compatibilidad y riesgos

- El cambio de modelo requiere aplicar la migración descrita en [data-model.md](./data-model.md) antes de desplegar código que dependa de la nueva columna.
- Las consultas de listado/detalle y los handlers de edición/estado excluirán automáticamente las propiedades retiradas.
- El acceso público sigue el patrón actual; no se añade autenticación.
- El archivo de imagen puede quedar sin referencia tras retirar el registro, deliberadamente, según la decisión de alcance.
- Los errores de persistencia no deben convertirse en éxito: únicamente una transición de una fila activa a retirada produce `200 OK`.
