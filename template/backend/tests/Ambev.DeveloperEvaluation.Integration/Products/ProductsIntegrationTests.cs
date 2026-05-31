using System.Net;
using System.Text;
using System.Text.Json;
using Ambev.DeveloperEvaluation.Integration.Infrastructure;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Integration.Products;

public class ProductsIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ProductsIntegrationTests(CustomWebApplicationFactory factory)
        => _client = factory.CreateClient();

    private StringContent Json(object obj) =>
        new(JsonSerializer.Serialize(obj), Encoding.UTF8, "application/json");

    private async Task<JsonDocument> ReadDoc(HttpResponseMessage r) =>
        JsonDocument.Parse(await r.Content.ReadAsStringAsync());

    private object ProductPayload(string title = "Test Product", string category = "electronics") => new
    {
        title,
        price = 99.95,
        description = "Integration test product",
        category,
        image = "https://img.com/test.jpg",
        rating = new { rate = 4.2, count = 100 }
    };

    [Fact(DisplayName = "POST /api/products — returns 201 with Rating value object")]
    public async Task CreateProduct_ValidPayload_Returns201WithRating()
    {
        var response = await _client.PostAsync("/api/products", Json(ProductPayload("Laptop Pro")));
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var doc = await ReadDoc(response);
        var data = doc.RootElement.GetProperty("data");
        data.GetProperty("title").GetString().Should().Be("Laptop Pro");
        data.GetProperty("rating").GetProperty("rate").GetDouble().Should().Be(4.2);
        data.GetProperty("rating").GetProperty("count").GetInt32().Should().Be(100);
    }

    [Fact(DisplayName = "GET /api/products/categories — returns distinct categories")]
    public async Task GetCategories_AfterInsert_ReturnsDistinctList()
    {
        await _client.PostAsync("/api/products", Json(ProductPayload("P1", "books")));
        await _client.PostAsync("/api/products", Json(ProductPayload("P2", "books")));
        await _client.PostAsync("/api/products", Json(ProductPayload("P3", "clothing")));

        var response = await _client.GetAsync("/api/products/categories");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var doc = await ReadDoc(response);
        var cats = doc.RootElement.GetProperty("data").EnumerateArray()
            .Select(e => e.GetString()).ToList();
        cats.Should().Contain("books").And.Contain("clothing");
        cats.Distinct().Should().HaveCount(cats.Count);
    }

    [Fact(DisplayName = "GET /api/products/category/{cat} — returns filtered products")]
    public async Task GetByCategory_FiltersByCategory()
    {
        await _client.PostAsync("/api/products", Json(ProductPayload("Sci-Fi Book", "books")));
        await _client.PostAsync("/api/products", Json(ProductPayload("T-Shirt", "clothing")));

        var response = await _client.GetAsync("/api/products/category/books");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var doc = await ReadDoc(response);
        doc.RootElement.GetProperty("data").EnumerateArray()
            .Should().AllSatisfy(p =>
                p.GetProperty("category").GetString().Should().Be("books"));
    }

    [Fact(DisplayName = "PUT /api/products/{id} — updates product correctly")]
    public async Task UpdateProduct_ValidPayload_UpdatesFields()
    {
        var create = await _client.PostAsync("/api/products", Json(ProductPayload("Original")));
        var created = await ReadDoc(create);
        var id = created.RootElement.GetProperty("data").GetProperty("id").GetString();

        var updated = new { title = "Updated", price = 49.99, description = "New desc", category = "electronics", image = "x", rating = new { rate = 3.5, count = 50 } };
        var response = await _client.PutAsync($"/api/products/{id}", Json(updated));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await ReadDoc(response);
        doc.RootElement.GetProperty("data").GetProperty("title").GetString().Should().Be("Updated");
        doc.RootElement.GetProperty("data").GetProperty("price").GetDecimal().Should().Be(49.99m);
    }

    [Fact(DisplayName = "DELETE /api/products/{id} — removes the product")]
    public async Task DeleteProduct_ExistingProduct_Returns200ThenNotFound()
    {
        var create = await _client.PostAsync("/api/products", Json(ProductPayload("ToDelete")));
        var created = await ReadDoc(create);
        var id = created.RootElement.GetProperty("data").GetProperty("id").GetString();

        var del = await _client.DeleteAsync($"/api/products/{id}");
        del.StatusCode.Should().Be(HttpStatusCode.OK);

        var get = await _client.GetAsync($"/api/products/{id}");
        get.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
