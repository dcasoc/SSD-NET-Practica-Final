# Investigación: eliminación de inmueble desde el listado Blazor

**Iniciativa**: `017-blazor-delete-property`

## Decisión 1: Respuesta de eliminación exitosa

**Decisión**: La eliminación correcta devolverá `200 OK` y el frontend retirará el inmueble al confirmar ese resultado.

**Justificación**: La especificación aclarada exige `200 OK`. El contrato acordado usa `200 OK` sin cuerpo como resultado exitoso, con `400` para identificadores inválidos y `404` para propiedades inexistentes.

**Alternativas consideradas**:

- Ninguna: el contrato queda definido como `200 OK` sin cuerpo para el éxito.

## Decisión 2: Confirmación en la tarjeta

**Decisión**: La confirmación se mostrará dentro de la tarjeta seleccionada, con el nombre del inmueble y las acciones Confirmar y Cancelar.

**Justificación**: Este comportamiento permite cancelar sin abandonar el listado e identifica claramente el elemento afectado.

**Alternativas consideradas**:

- Confirmación global fuera del listado: menos contextual y puede resultar ambigua.
- Confirmación en una ventana modal: no reutiliza directamente el patrón compacto solicitado para la tarjeta.

## Decisión 3: Prevención de duplicados

**Decisión**: La tarjeta bloqueará Confirmar y evitará una segunda llamada mientras la eliminación esté en curso.

**Justificación**: Es observable y evita solicitudes concurrentes duplicadas desde múltiples clics.

**Alternativas consideradas**:

- Permitir cada clic y confiar en el servidor: puede producir más tráfico y estados de UI incoherentes.
- Deshabilitar toda la tarjeta: sería más invasivo de lo necesario.

## Decisión 4: Recuperación ante error

**Decisión**: Ante error HTTP o de conexión, la tarjeta permanecerá visible, la confirmación seguirá disponible y se mostrará un mensaje comprensible.

**Justificación**: Conserva el contexto y permite reintentar sin recargar datos ni perder la imagen.

**Alternativas consideradas**:

- Ocultar la confirmación: dificulta la recuperación inmediata.
- Retirar la tarjeta ante error: produciría una eliminación visual incorrecta.

## Decisión 5: Actualización del listado

**Decisión**: Tras confirmar éxito, el frontend mostrará feedback y volverá a cargar el listado. También podrá retirar inmediatamente la tarjeta y ajustar la página vacía.

**Justificación**: La especificación exige que el inmueble desaparezca y que el listado refleje el estado posterior a la eliminación.

**Alternativas consideradas**:

- Retirar la tarjeta sin recargar: puede mostrar datos obsoletos si la base de datos rechazó o dividió la operación.
- No retirar hasta otra navegación: contradice el criterio de actualización visible.

## Decisión 6: Conservación de la imagen

**Decisión**: La operación eliminará únicamente la propiedad lógica y no modificará ni eliminará el recurso de imagen.

**Justificación**: Es una restricción explícita de la especificación y evita efectos secundarios fuera del borrado lógico.

**Alternativas consideradas**:

- Eliminar el recurso de imagen: fuera de alcance y con riesgo de afectar referencias compartidas.
