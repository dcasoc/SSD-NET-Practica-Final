---
name: 017-blazor-delete-property
agent: agent
---
Sigue exactamente el proceso, las reglas y el flujo interactivo definidos en:

.github\prompts\speckit-specify-prompt-interactive.prompt.md

Utiliza ese prompt como procedimiento principal para generar la nueva especificación.

La solicitud que debes procesar mediante ese flujo es la siguiente:

Quiero crear una funcionalidad que permita eliminar un inmueble existente. El nombre de la spec sera 017-blazor-delete-property.

Debera crearse el nuevo endpoint de eliminación, el cual recibira el ID del inmueble, y procedera a un borrado lógico del inmueble.

Dentro del componente app\frontend\src\RealtorWeb\Features\Properties\List\PropertyCard.razor, se debe agregar un botón o acción para eliminar el inmueble representado por la tarjeta. Al seleccionar esta acción, al usuario se le mostrara un pequeño mensaje donde se le pregunte si "¿Esta seguro de eliminar?" con dos botones para "Confirmar" que ejecutara la llamada al endpoint para el borrado, y otro boton "Cancelar" que ocultara este mensaje al usuario.

El mensaje de eliminación debe:

Mostrar la pregunta "¿Esta seguro de eliminar?".
Mostrar dos botones uno para "Confirmar", y otro para "Cancelar".
"Confirmar" que ejecutara la llamada al endpoint para el borrado, y con la respues 200 del endpoint se producira el borrado del listado en pantalla de ese inmueble.
"Cancelar" que ocultara este mensaje al usuario.
Consumir el endpoint cuyo operationId es DeleteProperty para guardar los cambios en el cambio de estado a borrado del inmueble.
Evitar el envío del formulario mientras existan errores de validación.
Mostrar estados visuales durante el proceso.
Mostrar una confirmación cuando la actualización se complete correctamente.
Mostrar un mensaje de error comprensible cuando la actualización no pueda realizarse.
Evitar que múltiples clics generen solicitudes duplicadas.
Mantener la opción de confirmación visible si el servidor devuelve un error.
Recargar el listado para no mostrar el inmueble después de una eliminacion exitosa.

La imagen que representa el inmueble debe conservarse.

La implementación debe reutilizar los componentes, estilos, servicios HTTP, modelos, sistema de diseño y patrones de manejo de errores que ya existen en app\frontend\src\RealtorWeb\RealtorWeb.csproj.

La especificación debe enfocarse en el comportamiento esperado y en los criterios de aceptación, sin definir todavía detalles internos de implementación.