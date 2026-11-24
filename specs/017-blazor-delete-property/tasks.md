# Tareas: Eliminación de inmueble desde Blazor

**Entrada**: `specs/017-blazor-delete-property/spec.md`, `plan.md`, `data-model.md`, `research.md` y `contracts/`

**Prerequisitos**: Plan y modelo persistente disponibles en la misma iniciativa.

**Pruebas**: Las historias incluyen criterios de prueba independientes; las pruebas automatizadas unitarias deben limitarse a los contratos y comportamientos existentes permitidos por la constitución.

## Fase 1: Preparación compartida

- [X] T001 Revisar la especificación, el plan, el modelo de datos y el contrato funcional de `specs/017-blazor-delete-property/` antes de implementar.
- [X] T002 Confirmar que la eliminación lógica usa `IsDeleted` y que la migración `20261123195817_SoftDeleteProperties` cumple `data-model.md` en `app/backend/src/RealtorApi/Infrastructure/Persistence/Migrations/20261123195817_SoftDeleteProperties.cs`.

## Fase 2: Fundaciones y contrato backend

**Propósito**: Alinear el endpoint y el contrato con el borrado lógico y la respuesta exitosa `200 OK`.

- [X] T003 Modificar `app/backend/src/RealtorApi/Features/Properties/DeleteProperty/DeletePropertySlice.cs` para responder `200 OK` sin cuerpo solamente después de retirar una propiedad activa; si no cambia ninguna fila, responder `404 Not Found`. Mantener `400` para UUID inválido.
- [X] T004 Mantener `400 Bad Request` y `404 Not Found` con ProblemDetails en `app/backend/src/RealtorApi/Features/Properties/DeleteProperty/DeletePropertySlice.cs`.
- [X] T005 Actualizar el metadata y el operationId `DeleteProperty` según el contrato público en `app/backend/src/RealtorApi/Features/Properties/DeleteProperty/DeletePropertySlice.cs`.
- [X] T006 Verificar que `app/backend/src/RealtorApi/Features/Properties/DeleteProperty/DeletePropertyHandler.cs` actualice condicionalmente solo una propiedad activa y conserve su imagen y demás datos.
- [X] T007 Alinear el contrato runtime y el documento publicado `app/backend/src/RealtorApi/wwwroot/openapi/v1.json`: `200 OK` sin contenido como éxito, `400` como ProblemDetails de validación y `404` como ProblemDetails de inexistencia.

**Checkpoint**: El endpoint puede distinguir éxito, identificador inválido e inexistencia sin alterar otras propiedades.

## Fase 3: Historia de Usuario 1 - Solicitar confirmación desde el listado (Prioridad: P1)

**Meta**: Permitir iniciar la eliminación desde la tarjeta y confirmar o cancelar sin enviar una solicitud accidental.

**Prueba independiente**: Activar Eliminar en una tarjeta, comprobar el texto con el nombre de la propiedad y validar que Cancelar oculta el mensaje sin llamada HTTP.

- [X] T008 [P] [US1] Ajustar la confirmación integrada en `app/frontend/src/RealtorWeb/Features/Properties/List/PropertyCard.razor` para mostrar exactamente «¿Esta seguro de eliminar la propiedad <nombre>?».
- [X] T009 [P] [US1] Añadir los botones Confirmar y Cancelar a la confirmación de `app/frontend/src/RealtorWeb/Features/Properties/List/PropertyCard.razor`.
- [X] T010 [US1] Aplicar estados visuales y separación compacta de acciones en `app/frontend/src/RealtorWeb/wwwroot/app.css`.
- [X] T011 [US1] Implementar `BeginDelete` y `CancelDelete` sin solicitud HTTP en `app/frontend/src/RealtorWeb/Features/Properties/List/PropertyCard.razor.cs`.

**Checkpoint**: Confirmar solo inicia después de una decisión explícita y Cancelar nunca modifica el listado.

## Fase 4: Historia de Usuario 2 - Confirmar la eliminación (Prioridad: P1)

**Meta**: Ejecutar una sola eliminación mediante el cliente tipado y retirar el inmueble después de `200 OK`.

**Prueba independiente**: Confirmar una propiedad existente y comprobar una sola llamada, `200 OK`, mensaje de éxito y desaparición de la tarjeta sin cambios en demás imágenes.

- [X] T012 [P] [US2] Añadir o ajustar `DeletePropertyAsync` con Refit DELETE en `app/frontend/src/RealtorWeb/Services/Api/Properties/IPropertiesApi.cs`.
- [X] T013 [US2] Implementar el bloqueo de clics simultáneos mediante el estado `isDeleting` en `app/frontend/src/RealtorWeb/Features/Properties/List/PropertyCard.razor.cs`.
- [X] T014 [US2] Ejecutar `DeletePropertyAsync` y tratar únicamente `200 OK` como resultado exitoso en `app/frontend/src/RealtorWeb/Features/Properties/List/PropertyCard.razor.cs`.
- [X] T015 [US2] Ocultar la confirmación y notificar al listado solamente después del éxito en `app/frontend/src/RealtorWeb/Features/Properties/List/PropertyCard.razor.cs`.
- [X] T016 [US2] Mostrar el mensaje de éxito arriba del listado y retirar la propiedad eliminada en `app/frontend/src/RealtorWeb/Features/Properties/List/PropertyList.razor.cs`.
- [X] T017 [US2] Recargar o ajustar la página actual cuando quede vacía después de la eliminación en `app/frontend/src/RealtorWeb/Features/Properties/List/PropertyList.razor.cs`.

**Checkpoint**: La respuesta `200 OK` elimina visualmente una sola tarjeta y mantiene las demás propiedades e imágenes.

## Fase 5: Historia de Usuario 3 - Recuperación ante errores (Prioridad: P1)

**Meta**: Mantener el contexto de la tarjeta ante errores y permitir reintento sin solicitudes duplicadas.

**Prueba independiente**: Forzar `400`, `404`, `500` o pérdida de conexión y comprobar tarjeta visible, confirmación disponible y mensaje comprensible.

- [X] T018 [P] [US3] Mantener `isConfirmingDelete` activo y mostrar el estado de error en `app/frontend/src/RealtorWeb/Features/Properties/List/PropertyCard.razor.cs`.
- [X] T019 [P] [US3] Mapear `404`, `400`, errores HTTP restantes y excepciones de conexión a mensajes comprensibles en `app/frontend/src/RealtorWeb/Features/Properties/List/PropertyCard.razor.cs`.
- [X] T020 [US3] Restablecer el estado de eliminación en `finally` y permitir un nuevo Confirmar sin modificar la tarjeta en `app/frontend/src/RealtorWeb/Features/Properties/List/PropertyCard.razor.cs`.
- [X] T021 [US3] Mostrar el mensaje de error junto a la tarjeta sin ocultar su imagen ni sus datos en `app/frontend/src/RealtorWeb/Features/Properties/List/PropertyCard.razor` y `app/frontend/src/RealtorWeb/wwwroot/app.css`.

**Checkpoint**: Cada error deja la propiedad disponible para reintento y no produce una eliminación visual prematura.

## Fase 6: Persistencia, validación y pulido

- [X] T022 Verificar que el filtro global y las consultas normales excluyan propiedades retiradas conforme a `specs/017-blazor-delete-property/data-model.md` en `app/backend/src/RealtorApi/Infrastructure/Persistence/AppDbContext.cs` y las configuraciones de entidad.
- [X] T023 Añadir o ajustar pruebas unitarias focalizadas de `DeletePropertyEndpointTests` en `app/backend/tests/RealtorApiTests/Infrastructure/Endpoints/DeletePropertyEndpointTests.cs` para `200 OK`, `400`, `404`, borrado lógico e integridad.
- [X] T024 [P] Ejecutar `support/scripts/generate-openapi-v1.ps1`, versionar `app/backend/src/RealtorApi/wwwroot/openapi/v1.json` y comprobar que `operationId=DeleteProperty` declara `200`, `400` y `404`; comparar el resultado con el contrato runtime.
- [X] T025 Ejecutar pruebas backend existentes y compilaciones de backend/frontend usando el flujo de pruebas .NET del repositorio. Resultado: 38 pruebas backend aprobadas, 1 prueba frontend aprobada y solución compila; existen advertencias de vulnerabilidades de paquetes y una advertencia BL0012 ajena al flujo.
- [X] T026 Ejecutar la validación manual definida en `specs/017-blazor-delete-property/quickstart.md`, incluyendo confirmación, cancelación, éxito, error, estado de carga y clics múltiples.
- [X] T027 Registrar el resultado de las validaciones en `specs/017-blazor-delete-property/quickstart.md` y resolver cualquier diferencia entre el contrato runtime y el contrato publicado.

## Dependencias

- La Fase 2 bloquea todas las historias de usuario.
- US1 puede implementarse después de la fundación y no depende de la lógica de éxito.
- US2 depende del endpoint de Fase 2 y del cliente Refit de T012.
- US3 depende de US2 para reutilizar el flujo de confirmación y recuperación.
- T022 depende de `data-model.md` y de T006; no debe editarse sin confirmar esas reglas persistentes.
- T024 depende de las modificaciones del endpoint y precede a la validación final del contrato.
- T027 depende de las validaciones T025 y T026.

## Ejemplos de ejecución en paralelo

### Preparación y fundación

- T008 y T009 pueden ejecutarse en paralelo dentro de US1 porque modifican distintas superficies de presentación.
- T018 y T019 pueden prepararse en paralelo dentro de US3 porque afectan bloques separados de comportamiento y presentación.
- T024 puede ejecutarse en paralelo con la preparación de pruebas una vez establecida la respuesta HTTP del endpoint.

### Ejemplos por historia

- **US1**: T008-T011.
- **US2**: T012-T017.
- **US3**: T018-T021.

## Estrategia de implementación

1. Completar las fundaciones backend y alinear `200 OK` en endpoint y OpenAPI.
2. Implementar US1 como MVP visual independiente de confirmación y cancelación.
3. Completar US2 para lograr el flujo exitoso completo con recarga y eliminación visual.
4. Completar US3 para recuperación ante fallos y reintentos.
5. Ejecutar persistencia, pruebas, contrato y validación manual antes del cierre.

## Criterios de formato

Todas las tareas siguen el formato requerido: checkbox, ID secuencial, marcador `[P]` solo cuando el trabajo puede ejecutarse en paralelo y etiqueta `[US1]`, `[US2]` o `[US3]` únicamente dentro de las fases de historia.