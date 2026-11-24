# Especificación de Funcionalidad: Eliminación de inmueble existente en frontend Blazor

**Rama de Funcionalidad**: `017-blazor-delete-property`

**Creado**: 2026-11-24

**Estado**: Implementada

**Entrada**: Crear una funcionalidad que permita eliminar un inmueble existente desde su tarjeta del listado. La eliminación será lógica, utilizará el identificador del inmueble y conservará la imagen.

## Ciclo de estado del spec *(obligatorio)*

- Estado inicial: **Borrador**
- Transiciones permitidas: **Borrador -> Aprobada -> En implementacion -> Implementada**
- Está prohibido cerrar en **Implementada** sin todas las tareas completadas en `tasks.md`
- Está prohibido cerrar en **Implementada** sin evidencia de validación en `quickstart.md`

## Trazabilidad de estado *(obligatorio)*

- 2026-11-24 Borrador - Especificación creada mediante `/speckit-specify-prompt-interactive`
- 2026-11-24 Aprobada -> En implementación - Inicio autorizado de implementación
- 2026-11-24 En implementación -> Implementada - Cierre validado: tareas completas y evidencia de validación registrada

## Clarificaciones

### Sesión 2026-11-24

- Q: ¿Qué respuesta HTTP debe considerarse exitosa al eliminar el inmueble? -> A: `200 OK`.
- Q: ¿Qué texto debe mostrar el cuadro de confirmación antes de eliminar? -> A: «¿Esta seguro de eliminar la propiedad <nombre>?».
- Q: ¿Qué debe ocurrir si la eliminación falla por un error del servidor? -> A: Mantener la confirmación visible y permitir reintento.
- Q: ¿Cómo debe resolverse la diferencia entre `200 OK` y el contrato anterior observado? -> A: El contrato acordado establece `200 OK` como respuesta exitosa, junto con `400` para identificadores inválidos y `404` para propiedades inexistentes.
- Q: ¿Cómo debe mostrarse la confirmación del éxito? -> A: Mostrar un mensaje breve arriba del listado y retirar inmediatamente la tarjeta eliminada.

## Contexto y objetivo

El catálogo permite consultar, crear y actualizar inmuebles, pero no ofrece una forma controlada de retirar uno existente desde su tarjeta. La iniciativa permite iniciar la eliminación desde el listado, solicitar confirmación explícita e informar al usuario del resultado sin permitir acciones accidentales o solicitudes duplicadas.

La persona operadora podrá eliminar un inmueble existente mediante borrado lógico. El recurso de imagen asociado permanecerá sin cambios.

## Alcance incluido

- Mostrar una acción de eliminación en cada tarjeta del listado.
- Solicitar confirmación antes de ejecutar la eliminación.
- Ejecutar una sola solicitud mientras la operación esté en curso.
- Informar el progreso, el éxito y los errores.
- Retirar el inmueble eliminado del listado después de una respuesta exitosa.
- Mantener la tarjeta y la opción de confirmación ante un error para permitir reintento.
- Conservar la imagen del inmueble.

## Historias de usuario y escenarios de aceptación

### Historia de Usuario 1 - Solicitar confirmación desde el listado (Prioridad: P1)

Como persona operadora del catálogo, quiero confirmar o cancelar la eliminación desde la tarjeta para evitar retiradas accidentales.

**Por qué esta prioridad**: Es la entrada principal al flujo y evita acciones destructivas involuntarias.

**Prueba independiente**: Activar Eliminar en una tarjeta, comprobar la pregunta y validar que Confirmar ejecuta la eliminación mientras Cancelar solamente oculta la confirmación.

**Escenarios de aceptación**:

1. **Dado** que una tarjeta ofrece la acción Eliminar, **cuando** la persona activa esa acción, **entonces** se muestra el mensaje «¿Esta seguro de eliminar la propiedad <nombre>?» con los botones Confirmar y Cancelar.
2. **Dado** que el mensaje de confirmación está visible, **cuando** la persona selecciona Cancelar, **entonces** el mensaje se oculta y no se envía ninguna solicitud.
3. **Dado** que la propiedad continúa disponible, **cuando** la persona cancela la confirmación, **entonces** la tarjeta conserva sus datos e imagen.

### Historia de Usuario 2 - Confirmar la eliminación (Prioridad: P1)

Como persona operadora del catálogo, quiero confirmar la eliminación de un inmueble para retirarlo del catálogo sin modificar otras propiedades.

**Por qué esta prioridad**: Representa el objetivo funcional principal de la iniciativa.

**Prueba independiente**: Confirmar la eliminación de una propiedad existente y comprobar la respuesta `200 OK`, el mensaje de éxito y la desaparición de la tarjeta.

**Escenarios de aceptación**:

1. **Dado** que la confirmación está visible para un inmueble existente, **cuando** la persona selecciona Confirmar, **entonces** el sistema inicia una única solicitud de eliminación para ese inmueble.
2. **Dado** que la solicitud está en curso, **cuando** la persona activa Confirmar varias veces, **entonces** no se envían solicitudes duplicadas.
3. **Dado** que el servicio responde `200 OK`, **cuando** finaliza la eliminación, **entonces** el sistema muestra durante 5 segundos una confirmación arriba del listado y retira la tarjeta correspondiente.
4. **Dado** que se elimina una propiedad, **cuando** se muestra el listado actualizado, **entonces** las demás propiedades conservan sus datos e imágenes.

### Historia de Usuario 3 - Recuperación ante errores (Prioridad: P1)

Como persona operadora del catálogo, quiero conocer cuando una eliminación no puede completarse y conservar la posibilidad de reintentar.

**Por qué esta prioridad**: Evita pérdida de contexto y permite recuperarse ante fallos temporales sin retirar información correcta del listado.

**Prueba independiente**: Forzar respuestas `400`, `404`, `500` o una pérdida de conexión y comprobar el mensaje, la permanencia de la tarjeta y la disponibilidad del reintento.

**Escenarios de aceptación**:

1. **Dado** que existe un error del servicio o de conexión, **cuando** finaliza el intento, **entonces** la tarjeta permanece visible y se muestra «No se pudo eliminar la propiedad. Inténtalo de nuevo.».
2. **Dado** que la eliminación falla, **cuando** se procesa el error, **entonces** la confirmación permanece visible y la acción Confirmar permite realizar un nuevo intento.
3. **Dado** que el inmueble ya no existe, **cuando** el servicio responde `404`, **entonces** la interfaz muestra «La propiedad ya no está disponible.» sin afirmar que fue eliminado correctamente.
4. **Dado** que el identificador no es válido, **cuando** el servicio responde `400`, **entonces** la interfaz muestra «El identificador de la propiedad no es válido.» y mantiene el listado estable.

### Casos límite

- Una segunda eliminación posterior de la misma propiedad se trata como propiedad inexistente.
- Si dos solicitudes intentan eliminar simultáneamente el mismo inmueble, solamente debe completarse una eliminación y la otra debe recibir el resultado correspondiente de inexistencia.
- Un inmueble puede tener o no una URL de imagen; ninguna de las opciones elimina el recurso de imagen.
- Una pérdida de conexión no debe retirar la tarjeta ni mostrar éxito.
- Después de una eliminación exitosa, el inmueble no debe aparecer en el listado ni en consultas posteriores.
- Cancelar la confirmación no modifica el listado ni genera una solicitud.
- Los múltiples clics durante una solicitud en curso no generan solicitudes adicionales.

## Requisitos *(obligatorio)*

### Requisitos funcionales

- **FR-001**: El sistema DEBE eliminar el inmueble que coincida con el identificador recibido.
- **FR-002**: La eliminación DEBE realizarse como borrado lógico y no DEBE eliminar ni modificar el recurso de imagen asociado.
- **FR-003**: El sistema DEBE responder `200 OK` cuando la eliminación se complete correctamente.
- **FR-004**: Cuando el inmueble no exista, el sistema DEBE informar que no fue encontrado sin modificar otros inmuebles.
- **FR-005**: Cuando el identificador no tenga un formato válido, el sistema DEBE informar que la solicitud es inválida.
- **FR-006**: Después de una eliminación exitosa, el inmueble NO DEBE aparecer en el listado ni en consultas normales posteriores.
- **FR-007**: La interfaz DEBE mostrar la acción Eliminar en la tarjeta de cada inmueble del listado.
- **FR-008**: La interfaz DEBE mostrar exactamente el mensaje «¿Esta seguro de eliminar la propiedad <nombre>?» antes de ejecutar la eliminación.
- **FR-009**: La interfaz DEBE mostrar los botones Confirmar y Cancelar dentro del mensaje de confirmación.
- **FR-010**: Seleccionar Cancelar DEBE ocultar el mensaje sin enviar una solicitud ni modificar el inmueble.
- **FR-011**: Seleccionar Confirmar DEBE iniciar la llamada de eliminación una sola vez para el inmueble seleccionado.
- **FR-012**: La interfaz DEBE mostrar un estado visual mientras la solicitud de eliminación esté en curso.
- **FR-013**: La interfaz DEBE evitar solicitudes duplicadas mientras exista una eliminación en curso para el mismo inmueble.
- **FR-014**: Ante `200 OK`, la interfaz DEBE mostrar la confirmación arriba del listado durante 5 segundos y recargarlo para retirar el inmueble eliminado.
- **FR-015**: Ante un error del servicio o de conexión, la interfaz DEBE conservar la tarjeta y mantener visible la confirmación para permitir reintento.
- **FR-016**: Ante `404`, la interfaz DEBE mostrar «La propiedad ya no está disponible.»; ante `400`, «El identificador de la propiedad no es válido.»; ante otros errores o pérdida de conexión, «No se pudo eliminar la propiedad. Inténtalo de nuevo.».
- **FR-017**: La interfaz DEBE conservar las imágenes de todos los inmuebles que no hayan sido eliminados.
- **FR-018**: La eliminación de un inmueble NO DEBE alterar los datos de otros inmuebles del catálogo.

### Reglas de validación funcional basadas en contrato

- **RV-001**: El identificador utilizado para eliminar corresponde al inmueble representado por la tarjeta seleccionada.
- **RV-002**: Confirmar conserva el proceso activo hasta recibir una respuesta de éxito o error del servicio.
- **RV-003**: Cancelar tiene prioridad para ocultar el mensaje cuando no existe una solicitud de eliminación en curso.

## Contrato observable del servicio

La operación pública `DeleteProperty` acepta el identificador del inmueble y realiza el borrado lógico. Devuelve `200 OK` cuando elimina el registro, `404 Not Found` cuando no existe y `400 Bad Request` cuando el identificador no es válido.

### Entidades clave

- **Acción de eliminación**: Acción disponible en cada tarjeta del listado para iniciar el flujo de eliminación del inmueble representado.
- **Confirmación de eliminación**: Mensaje integrado en la tarjeta que identifica el inmueble y ofrece Confirmar y Cancelar.
- **Estado de eliminación**: Estado temporal visible mientras el servicio procesa la solicitud y que impide solicitudes duplicadas.
- **Resultado de eliminación**: Confirmación visible y listado actualizado tras éxito, o mensaje de error con reintento disponible ante fallo.

## Fuera de alcance

- Eliminar o modificar el recurso de imagen asociado al inmueble.
- Añadir eliminación desde la página de detalle.
- Incorporar papelera, restauración o historial visible.
- Implementar eliminación masiva.
- Cambiar los campos del inmueble, la paginación o las reglas de autenticación existentes.

## Criterios de éxito *(obligatorio)*

- **CE-001**: En el 100% de las eliminaciones confirmadas con datos válidos, el servicio devuelve `200 OK` y el inmueble deja de aparecer en el listado.
- **CE-002**: En el 100% de las cancelaciones, no se envía ninguna solicitud y el inmueble permanece visible.
- **CE-003**: En el 100% de las solicitudes con estado visual activo, la interfaz impide clics adicionales que produzcan solicitudes duplicadas.
- **CE-004**: Ante `400`, `404`, `500` o pérdida de conexión, la interfaz conserva la tarjeta, mantiene la confirmación visible y muestra un mensaje comprensible.
- **CE-005**: Tras una eliminación exitosa, el inmueble deja de aparecer en listado y consultas normales sin modificar las demás propiedades o sus imágenes.

## Suposiciones

- Existe un contrato público para el identificador del inmueble identificado por la tarjeta.
- Los patrones existentes del frontend permiten mostrar confirmación, progreso, errores y éxito sin rediseñar el sistema visual global.
- El listado puede volver a cargarse después de una eliminación exitosa.
