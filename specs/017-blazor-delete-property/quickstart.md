# Quickstart: validación de eliminación de inmueble

**Iniciativa**: `017-blazor-delete-property`

## Prerrequisitos

- SDK .NET configurado por `global.json`.
- Base de datos PostgreSQL disponible para ejecutar el backend si se valida persistencia real.
- Backend y frontend preparados para ejecutar la solución `app/Realtor.sln`.

## Validaciones manuales

1. Abrir el listado de inmuebles.
2. Seleccionar Eliminar en una tarjeta existente.
3. Comprobar que aparece exactamente: «¿Esta seguro de eliminar la propiedad <nombre>?».
4. Seleccionar Cancelar y comprobar que no cambia el listado ni se envía una solicitud.
5. Seleccionar Eliminar y confirmar.
6. Comprobar el estado visual durante la solicitud y que clics adicionales no generan otra solicitud.
7. Ante `200 OK`, comprobar que aparece confirmación arriba del listado durante 5 segundos y que la tarjeta desaparece tras recarga.
8. Repetir para un inmueble ya retirado y comprobar el mensaje de inexistencia sin modificar otros elementos.
9. Forzar `400` y comprobar «El identificador de la propiedad no es válido.»; forzar `404` y comprobar «La propiedad ya no está disponible.»; forzar `500` o pérdida de conexión y comprobar «No se pudo eliminar la propiedad. Inténtalo de nuevo.»; en todos los fallos, la tarjeta, su imagen y su confirmación deben permanecer visibles.

## Validaciones automatizadas previstas

- Compilar el backend y el frontend.
- Ejecutar las pruebas existentes del backend y frontend.
- Ejecutar las pruebas focalizadas del endpoint de eliminación.
- Verificar las respuestas `200 OK` sin cuerpo, `400 Bad Request` y `404 Not Found`, y confirmar que coinciden runtime y OpenAPI.
- Verificar que una propiedad retirada no aparece en listado ni detalle y que una segunda eliminación recibe inexistencia.
- Verificar que la migración mantiene `IsDeleted=false` en registros existentes y que el filtro global excluye registros retirados.

## Evidencia de validación

Fecha: 2026-11-24.

- **Pruebas backend focalizadas**: `dotnet test app/backend/tests/RealtorApiTests/RealtorApiTests.csproj --filter "FullyQualifiedName~DeletePropertyEndpointTests"` resultó en 5 pruebas aprobadas y 0 fallidas.
- **Suite backend completa**: `dotnet test app/backend/tests/RealtorApiTests/RealtorApiTests.csproj` resultó en 38 pruebas aprobadas y 0 fallidas.
- **Prueba frontend**: `dotnet test app/frontend/test/RealtorWeb/RealtorWebTests.csproj` resultó en 1 prueba aprobada y 0 fallidas.
- **Compilación de solución**: `dotnet build app/Realtor.sln --no-restore --tl:off` terminó correctamente con 0 errores.
- **Contrato OpenAPI**: `support/scripts/generate-openapi-v1.ps1` regeneró `v1.json`, pasó Redocly con 9 advertencias documentales y pasó NSwag. La operación `DeleteProperty` publicó `200`, `400` y `404`.
- **Backend y Supabase**: el reintento con la nueva contraseña inició correctamente en `http://localhost:5023`, aplicó las migraciones pendientes, incluidas las de borrado lógico, publicó `/health` como `ok` y devolvió 10 inmuebles desde el listado.
- **Cancelación manual**: desde `http://localhost:5025/`, se abrió la confirmación de «Casa amplia en Westchester», se seleccionó «Cancelar» y la tarjeta permaneció visible sin cambio.
- **Confirmación manual**: se confirmaron eliminaciones sobre «Casa clásica en The Roads» y «Casa Coral Way con jardín tropical». Las acciones quedaron deshabilitadas con «Eliminando...», el listado pasó de 10 a 9 y de 9 a 8 elementos, y las tarjetas desaparecieron.
- **Resultado HTTP**: la confirmación de borrado recibió `200 OK`; el detalle de «Casa clásica en The Roads» quedó como `404`; una repetición válida sobre el identificador ya retirado respondió `404`, y un UUID inválido respondió `400`.
- **Error de conexión manual**: con el backend detenido, se confirmó la eliminación de «Casa amplia en Westchester». El flujo conservó la tarjeta e imagen, mantuvo visibles Confirmar y Cancelar y mostró exactamente «No se pudo eliminar la propiedad. Inténtalo de nuevo.». Al reiniciar el backend, `/health` volvió a responder `ok`.
- **Evidencia visual**: el navegador mostró el listado real de Supabase, confirmaciones contextuales, el estado «Eliminando...», la reducción del número de elementos y el reintento recuperable ante pérdida de conexión.