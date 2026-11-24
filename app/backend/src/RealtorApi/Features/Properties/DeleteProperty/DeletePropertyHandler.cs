using Microsoft.EntityFrameworkCore;
using RealtorApi.Infrastructure.Handlers;
using RealtorApi.Infrastructure.Persistence;

namespace RealtorApi.Features.Properties.DeleteProperty;

public sealed class DeletePropertyHandler(
    AppDbContext dbContext,
    ILogger<DeletePropertyHandler> logger) : IHandler
{
    internal async Task<bool> HandleAsync(
        DeletePropertyCommand command,
        CancellationToken cancellationToken)
    {
        if (command.Id is null)
        {
            throw new InvalidOperationException("DeletePropertyHandler recibió un id no validado.");
        }

        var deletedAt = DateTimeOffset.UtcNow;
        var deletedCount = await dbContext.Properties
            .Where(property => property.Id == command.Id.Value && !property.IsDeleted)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(property => property.IsDeleted, true)
                    .SetProperty(property => property.UpdatedAt, deletedAt),
                cancellationToken);

        if (deletedCount == 0)
        {
            logger.LogInformation(
                "Eliminación lógica de propiedad: no encontrada. PropertyId={PropertyId}",
                command.Id.Value);

            return false;
        }

        logger.LogInformation(
            "Propiedad retirada correctamente. PropertyId={PropertyId}",
            command.Id.Value);

        return true;
    }
}
