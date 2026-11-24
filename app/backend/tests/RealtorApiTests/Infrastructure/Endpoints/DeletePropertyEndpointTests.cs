using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RealtorApi.Domain.Properties;
using RealtorApi.Infrastructure.Persistence;
using RealtorApiTests.Infrastructure;

namespace RealtorApiTests.Infrastructure.Endpoints;

public sealed class DeletePropertyEndpointTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory _factory;

    public DeletePropertyEndpointTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Delete_Returns200WithoutBody_AndHidesSoftDeletedPropertyFromCatalog()
    {
        var property = await CreatePropertyAsync("Propiedad a eliminar", "/assets/properties/original.png");
        var otherProperty = await CreatePropertyAsync("Propiedad que permanece");

        var response = await _client.DeleteAsync($"/api/properties/{property.Id:D}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Empty(await response.Content.ReadAsByteArrayAsync());

        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var storedProperty = await dbContext.Properties
                .IgnoreQueryFilters()
                .SingleAsync(item => item.Id == property.Id);

            Assert.True(storedProperty.IsDeleted);
        }

        var detailResponse = await _client.GetAsync($"/api/properties/{property.Id:D}");
        Assert.Equal(HttpStatusCode.NotFound, detailResponse.StatusCode);

        var retainedResponse = await _client.GetAsync($"/api/properties/{otherProperty.Id:D}");
        Assert.Equal(HttpStatusCode.OK, retainedResponse.StatusCode);

        var listResponse = await _client.GetAsync("/api/properties?page=1&pageSize=1000");
        listResponse.EnsureSuccessStatusCode();
        using var list = JsonDocument.Parse(await listResponse.Content.ReadAsStringAsync());
        var listedIds = list.RootElement
            .GetProperty("items")
            .EnumerateArray()
            .Select(item => item.GetProperty("id").GetGuid())
            .ToArray();

        Assert.DoesNotContain(property.Id, listedIds);
        Assert.Contains(otherProperty.Id, listedIds);
    }

    [Fact]
    public async Task Delete_Returns400ProblemDetails_WhenIdIsInvalid()
    {
        var response = await _client.DeleteAsync("/api/properties/not-a-uuid");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Delete_Returns404ProblemDetails_WhenPropertyDoesNotExistOrWasAlreadyDeleted()
    {
        var property = await CreatePropertyAsync("Propiedad eliminada previamente");
        var firstResponse = await _client.DeleteAsync($"/api/properties/{property.Id:D}");
        var secondResponse = await _client.DeleteAsync($"/api/properties/{property.Id:D}");
        var missingResponse = await _client.DeleteAsync($"/api/properties/{Guid.NewGuid():D}");

        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, secondResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, missingResponse.StatusCode);
        Assert.Equal("application/problem+json", secondResponse.Content.Headers.ContentType?.MediaType);

        var detailResponse = await _client.GetAsync($"/api/properties/{property.Id:D}");
        Assert.Equal(HttpStatusCode.NotFound, detailResponse.StatusCode);
    }

    [Fact]
    public async Task SoftDeletedProperty_CannotBeUpdatedOrHaveItsStatusChanged()
    {
        var property = await CreatePropertyAsync("Propiedad retirada");
        var path = $"/api/properties/{property.Id:D}";
        var deleteResponse = await _client.DeleteAsync(path);

        using var updateContent = new MultipartFormDataContent
        {
            { new StringContent("Título nuevo"), "Title" },
            { new StringContent("Descripción suficientemente larga."), "Description" },
            { new StringContent("Dirección de prueba"), "Address" },
            { new StringContent("1200"), "Price" },
            { new StringContent("Available"), "Status" }
        };
        var updateResponse = await _client.PutAsync(path, updateContent);
        var statusResponse = await _client.PatchAsJsonAsync(
            $"{path}/status",
            new { status = (int)PropertyStatusValue.Rented });

        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, updateResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, statusResponse.StatusCode);
    }

    [Fact]
    public async Task Delete_ConcurrentRequests_ReturnOne200AndOne404()
    {
        var property = await CreatePropertyAsync("Propiedad eliminada concurrentemente");
        var path = $"/api/properties/{property.Id:D}";

        var responses = await Task.WhenAll(
            _client.DeleteAsync(path),
            _client.DeleteAsync(path));

        Assert.Equal(1, responses.Count(response => response.StatusCode == HttpStatusCode.OK));
        Assert.Equal(1, responses.Count(response => response.StatusCode == HttpStatusCode.NotFound));

        var detailResponse = await _client.GetAsync(path);
        Assert.Equal(HttpStatusCode.NotFound, detailResponse.StatusCode);
    }

    private async Task<Property> CreatePropertyAsync(string title, string? imageUrl = null)
    {
        var property = new Property
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = "Descripción de prueba de una propiedad.",
            Address = "Calle de prueba 123",
            Price = 1000,
            Status = PropertyStatusValue.Available,
            BedroomCount = 2,
            BathroomCount = 1,
            AreaSquareMeters = 80,
            ImageUrl = imageUrl,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        dbContext.Properties.Add(property);
        await dbContext.SaveChangesAsync();

        return property;
    }
}
