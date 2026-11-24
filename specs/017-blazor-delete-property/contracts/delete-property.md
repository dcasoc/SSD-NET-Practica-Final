# Contrato funcional: eliminación de inmueble

## Operación

La interfaz solicita eliminar el inmueble identificado por el identificador de la tarjeta mediante el identificador público de la propiedad.

## Respuestas observables

- `200 OK`: eliminación completada sin cuerpo. La interfaz muestra confirmación durante 5 segundos, recarga el listado y deja de mostrar el inmueble eliminado.
- `400 Bad Request`: el identificador no es válido. La interfaz conserva el listado y muestra «El identificador de la propiedad no es válido.».
- `404 Not Found`: el inmueble no existe o ya fue retirado. La interfaz muestra «La propiedad ya no está disponible.» y conserva las demás propiedades.
- `500` o error de conexión: la interfaz conserva la tarjeta, mantiene visible la confirmación y muestra «No se pudo eliminar la propiedad. Inténtalo de nuevo.».

## Comportamiento de usuario

- El mensaje muestra: «¿Esta seguro de eliminar la propiedad <nombre>?».
- Los botones disponibles son Confirmar y Cancelar.
- Cancelar oculta el mensaje sin solicitar eliminación.
- Confirmar inicia una sola solicitud mientras no exista otra solicitud activa para la misma tarjeta.
- Durante la solicitud se muestra progreso y las acciones se bloquean contra clics duplicados.
- Tras éxito, se muestra confirmación durante 5 segundos arriba del listado y la tarjeta desaparece.
- Tras error, la tarjeta conserva su imagen y datos, y se mantiene el reintento.

## Conservación

- No se elimina ni modifica la imagen del inmueble.
- No se modifican las demás propiedades del listado.
