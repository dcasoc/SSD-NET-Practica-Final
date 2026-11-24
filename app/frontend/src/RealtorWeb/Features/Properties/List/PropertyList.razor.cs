using Microsoft.AspNetCore.Components;
using RealtorWeb.Services.Api.Properties;

namespace RealtorWeb.Features.Properties.List;

public enum PropertyListStateType
{
    Loading,
    Empty,
    Error
}

public partial class PropertyList : ComponentBase, IAsyncDisposable
{
    private const int DefaultPageSize = 9;

    private CancellationTokenSource? cancellationTokenSource;
    private CancellationTokenSource? successCancellationTokenSource;
    private Task? successClearTask;
    private PagedPropertyListResponse? response;
    private string? errorMessage;
    private string? successMessage;
    private bool isLoading = true;
    private int currentPage = 1;
    private long requestVersion;

    [Inject] private IPropertiesApi PropertiesApi { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        await LoadPageAsync(currentPage);
    }

    private bool CanGoNext => response?.HasNext == true;

    private bool CanGoPrevious => response?.HasPrevious == true;

    private async Task LoadPageAsync(int page)
    {
        requestVersion++;
        var version = requestVersion;

        if (cancellationTokenSource is not null)
        {
            await cancellationTokenSource.CancelAsync();
            cancellationTokenSource.Dispose();
        }

        cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        isLoading = true;
        errorMessage = null;
        StateHasChanged();

        try
        {
            var pageResponse = await PropertiesApi.GetPropertiesAsync(page, DefaultPageSize, cancellationToken);

            if (version != requestVersion)
            {
                return;
            }

            response = pageResponse;
            currentPage = pageResponse.Page;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return;
        }
        catch (Exception)
        {
            if (version != requestVersion)
            {
                return;
            }

            response = null;
            errorMessage = "No fue posible recuperar las propiedades. Intenta nuevamente.";
        }
        finally
        {
            if (version == requestVersion)
            {
                isLoading = false;
                StateHasChanged();
            }
        }
    }

    private Task ReloadAsync() => LoadPageAsync(currentPage);

    private async Task HandlePropertyDeletedAsync(PropertyListItem property)
    {
        successMessage = $"La propiedad «{property.Title}» se eliminó correctamente.";

        if (response is not null)
        {
            var remainingItems = response.Items
                .Where(item => item.Id != property.Id)
                .ToArray();

            if (remainingItems.Length != response.Items.Count)
            {
                response = response with
                {
                    Items = remainingItems,
                    TotalItems = Math.Max(0, response.TotalItems - 1)
                };
            }
        }

        await LoadPageAsync(currentPage);

        if (response is { Items.Count: 0 } && currentPage > 1)
        {
            await LoadPageAsync(currentPage - 1);
        }

        successClearTask = ClearSuccessMessageAfterDelay();
    }

    private async Task ClearSuccessMessageAfterDelay()
    {
        successCancellationTokenSource?.Cancel();
        successCancellationTokenSource?.Dispose();
        successCancellationTokenSource = new CancellationTokenSource();

        try
        {
            await Task.Delay(TimeSpan.FromSeconds(5), successCancellationTokenSource.Token);
            successMessage = null;
            await InvokeAsync(StateHasChanged);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private Task NextAsync()
    {
        if (!CanGoNext)
        {
            return Task.CompletedTask;
        }

        return LoadPageAsync(currentPage + 1);
    }

    private Task PreviousAsync()
    {
        if (!CanGoPrevious)
        {
            return Task.CompletedTask;
        }

        return LoadPageAsync(currentPage - 1);
    }

    public async ValueTask DisposeAsync()
    {
        if (cancellationTokenSource is not null)
        {
            await cancellationTokenSource.CancelAsync();
            cancellationTokenSource.Dispose();
        }

        if (successCancellationTokenSource is not null)
        {
            await successCancellationTokenSource.CancelAsync();
        }

        if (successClearTask is not null)
        {
            await successClearTask;
        }

        successCancellationTokenSource?.Dispose();
    }
}
