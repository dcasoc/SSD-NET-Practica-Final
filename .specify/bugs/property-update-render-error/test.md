# Bug Verification: Render inválido en PropertyUpdateForm por `return;` dentro del markup

- **Slug**: property-update-render-error
- **Tested**: 2026-07-25
- **Assessment**: ./assessment.md
- **Fix**: ./fix.md
- **Result**: partial

## Summary

El fix elimina de forma estructural la causa raíz: al quitar los `return;` del markup y usar
ramas `@if / else if / else`, el árbol de render siempre cierra `<article>` y `<div class="card-body">`,
por lo que la condición "A frame of type 'Element' was left unclosed" ya no puede producirse por
esa vía. La compilación es exitosa y las pruebas existentes pasan sin regresión. Sin embargo, la
reproducción original (navegar a `/properties/{id}/edit` y renderizar el componente en runtime)
**no fue ejercitada** de forma automatizada ni en la aplicación en ejecución, por lo que el
resultado se marca como `partial` y requiere validación manual final.

## Checks Performed

| Check | Command / Action | Result | Notes |
|-------|------------------|--------|-------|
| Reproducción runtime (post-fix) | Navegar a `/properties/{id}/edit` en la app en ejecución | not-run | No se ejecutó la app ni un render automatizado; la prueba bUnit quedó diferida por gobernanza (ver `fix.md`). |
| Compilación frontend | `dotnet build app/frontend/src/RealtorWeb/RealtorWeb.csproj -c Debug` | pass | Build succeeded, 0 Warning(s), 0 Error(s). |
| Pruebas frontend (regresión) | `dotnet test app/frontend/test/RealtorWeb/RealtorWebTests.csproj -c Debug` | pass | Passed: 1, Failed: 0, Skipped: 0. |
| Diagnóstico del editor | Análisis de `PropertyUpdateForm.razor` | pass | Sin errores reportados. |
| Análisis estructural del render | Revisión de balance OpenElement/CloseElement tras el fix | pass | Sin `return;` en markup; ramas mutuamente excluyentes garantizan cierre de elementos. |

## Output Excerpts

Compilación:

```text
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

Pruebas:

```text
Passed!  - Failed:     0, Passed:     1, Skipped:     0, Total:     1, Duration: 462 ms - RealtorWebTests.dll (net11.0)
```

Nota: `dotnet test` emitió `NU1902` (vulnerabilidad moderada conocida en
`Microsoft.AspNetCore.Components` 7.0.3 vía dependencia transitiva). No está relacionada con este
bug, pero conviene registrarla como hallazgo aparte.

## Residual Risks

- La validación runtime del render no se automatizó: `dotnet build` confirma que compila, pero la
  excepción original era de **render en tiempo de ejecución**, no de compilación. La garantía
  actual es estructural (balance de elementos), no una ejecución observada.
- Sin prueba de regresión bUnit, una futura reintroducción de `return;`/`try-catch` en el markup
  podría volver a romper el render sin ser detectada en CI.

## Recommendation

Hold — validación mayormente completada por análisis estructural + compilación + pruebas verdes,
pero **repetir el escenario manual** antes de cerrar el bug:

1. Iniciar frontend + backend.
2. Abrir listado, seleccionar **Editar** en una propiedad existente.
3. Confirmar que se muestra el estado de carga y luego el formulario con los datos actuales (sin
   `InvalidOperationException`).
4. Forzar un `id` inexistente y confirmar el estado `LoadFailed` con acciones Reintentar/Volver.
5. Editar campos válidos, guardar y confirmar navegación al detalle actualizado.

Si el escenario manual pasa, promover el resultado a `verified`. En paralelo, dar seguimiento a la
tarea diferida para agregar bUnit y la prueba de render de todos los estados (ver `fix.md`).
