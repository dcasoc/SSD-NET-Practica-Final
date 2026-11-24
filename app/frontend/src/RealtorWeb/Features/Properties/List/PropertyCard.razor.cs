using Microsoft.AspNetCore.Components;
using RealtorWeb.Services.Api.Properties;

namespace RealtorWeb.Features.Properties.List;

public partial class PropertyCard : ComponentBase
{
    private bool isConfirmingDelete;
    private bool isDeleting;
    private string? deleteErrorMessage;

    [Parameter]
    public PropertyListItem property { get; set; } = default!;

    [Parameter]
    public EventCallback<PropertyListItem> OnDeleted { get; set; }

    [Inject] private IPropertiesApi PropertiesApi { get; set; } = default!;

    [Inject] private ILogger<PropertyCard> Logger { get; set; } = default!;

    private void BeginDelete()
    {
        deleteErrorMessage = null;
        isConfirmingDelete = true;
    }

    private void CancelDelete()
    {
        if (isDeleting)
        {
            return;
        }

        isConfirmingDelete = false;
        deleteErrorMessage = null;
    }

    private async Task ConfirmDeleteAsync()
    {
        if (!isConfirmingDelete || isDeleting)
        {
            return;
        }

        isDeleting = true;
        deleteErrorMessage = null;

        try
        {
            using var response = await PropertiesApi.DeletePropertyAsync(property.Id.ToString("D"));

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                isConfirmingDelete = false;
                await OnDeleted.InvokeAsync(property);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                Logger.LogInformation(
                    "La propiedad {PropertyId} ya no existe al intentar eliminarla.",
                    property.Id);
                deleteErrorMessage = "La propiedad ya no está disponible.";
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                Logger.LogWarning(
                    "La API rechazó el identificador de la propiedad {PropertyId}.",
                    property.Id);
                deleteErrorMessage = "El identificador de la propiedad no es válido.";
            }
            else
            {
                Logger.LogWarning(
                    "La API rechazó la eliminación de la propiedad {PropertyId} con estado HTTP {StatusCode}.",
                    property.Id,
                    (int)response.StatusCode);
                deleteErrorMessage = "No se pudo eliminar la propiedad. Inténtalo de nuevo.";
            }
        }
        catch (Exception exception)
        {
            Logger.LogError(
                exception,
                "No fue posible completar la solicitud de eliminación de la propiedad {PropertyId}.",
                property.Id);
            deleteErrorMessage = "No se pudo eliminar la propiedad. Inténtalo de nuevo.";
        }
        finally
        {
            isDeleting = false;
        }
    }
}
