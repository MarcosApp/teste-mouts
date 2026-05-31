using System.Net;
using System.Text;
using System.Text.Json;
using Ambev.DeveloperEvaluation.Integration.Infrastructure;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Integration.Sales;

/// <summary>
/// Integration tests for the Sales API.
/// Uses WebApplicationFactory with an in-memory database — no real PostgreSQL needed.
/// Each test creates its own uniquely-numbered sale to avoid state conflicts.
/// </summary>
public class SalesIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public SalesIntegrationTests(CustomWebApplicationFactory factory)
        => _client = factory.CreateClient();

    private StringContent Json(object obj) =>
        new(JsonSerializer.Serialize(obj), Encoding.UTF8, "application/json");

    private async Task<JsonDocument> Doc(HttpResponseMessage r) =>
        JsonDocument.Parse(await r.Content.ReadAsStringAsync());

    private string UniqueNumber() => $"IT-{Guid.NewGuid():N}".Substring(0, 14);

    private object Sale(string number, int qty1 = 3, int qty2 = 10) => new
    {
        saleNumber = number,
        saleDate = "2026-05-31T00:00:00Z",
        customerId = "aaaa0001-0000-0000-0000-000000000001",
        customerName = "Integration Test",
        branchId = "bbbb0001-0000-0000-0000-000000000001",
        branchName = "Branch A",
        items = new[]
        {
            new { productId = "cccc0001-0000-0000-0000-000000000001", productName = "Prod A", quantity = qty1, unitPrice = 100.0 },
            new { productId = "cccc0001-0000-0000-0000-000000000002", productName = "Prod B", quantity = qty2, unitPrice = 50.0 }
        }
    };

    private async Task<(string id, JsonElement data)> CreateSale(object payload)
    {
        var r = await _client.PostAsync("/api/sales", Json(payload));
        r.StatusCode.Should().Be(HttpStatusCode.Created);
        var doc = await Doc(r);
        var data = doc.RootElement.GetProperty("data");
        return (data.GetProperty("id").GetString()!, data);
    }

    // ── CREATE ────────────────────────────────────────────────────────
    [Fact(DisplayName = "POST /api/sales — creates sale and returns 201")]
    public async Task Post_ValidSale_Returns201()
    {
        var num = UniqueNumber();
        var (id, data) = await CreateSale(Sale(num));

        id.Should().NotBeNullOrEmpty();
        data.GetProperty("saleNumber").GetString().Should().Be(num);
        data.GetProperty("isCancelled").GetBoolean().Should().BeFalse();
    }

    [Fact(DisplayName = "POST /api/sales — applies discount tiers correctly")]
    public async Task Post_DisountTiers_Correct()
    {
        var (_, data) = await CreateSale(Sale(UniqueNumber(), qty1: 3, qty2: 10));

        // qty=3 × R$100 = R$300 (no discount)
        // qty=10 × R$50 × 0.80 = R$400 (20% discount)
        data.GetProperty("totalAmount").GetDecimal().Should().Be(700m);

        var items = data.GetProperty("items").EnumerateArray().ToList();
        items.First(i => i.GetProperty("quantity").GetInt32() == 3)
             .GetProperty("discount").GetDecimal().Should().Be(0m);
        items.First(i => i.GetProperty("quantity").GetInt32() == 10)
             .GetProperty("discount").GetDecimal().Should().Be(0.20m);
    }

    [Fact(DisplayName = "POST /api/sales — qty = 4 applies 10% discount")]
    public async Task Post_Qty4_TenPercentDiscount()
    {
        var payload = new
        {
            saleNumber = UniqueNumber(),
            saleDate = "2026-05-31T00:00:00Z",
            customerId = "aaaa0001-0000-0000-0000-000000000001",
            customerName = "T", branchId = "bbbb0001-0000-0000-0000-000000000001", branchName = "B",
            items = new[] { new { productId = "cccc0001-0000-0000-0000-000000000001", productName = "P", quantity = 4, unitPrice = 100.0 } }
        };
        var (_, data) = await CreateSale(payload);

        // qty=4 × R$100 × 0.90 = R$360
        data.GetProperty("totalAmount").GetDecimal().Should().Be(360m);
        data.GetProperty("items").EnumerateArray().First()
            .GetProperty("discount").GetDecimal().Should().Be(0.10m);
    }

    [Fact(DisplayName = "POST /api/sales — qty > 20 is rejected with 400")]
    public async Task Post_QuantityAbove20_Returns400()
    {
        var payload = new
        {
            saleNumber = UniqueNumber(),
            saleDate = "2026-05-31T00:00:00Z",
            customerId = "aaaa0001-0000-0000-0000-000000000001",
            customerName = "T", branchId = "bbbb0001-0000-0000-0000-000000000001", branchName = "B",
            items = new[] { new { productId = "cccc0001-0000-0000-0000-000000000001", productName = "P", quantity = 21, unitPrice = 10.0 } }
        };

        var r = await _client.PostAsync("/api/sales", Json(payload));
        r.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "POST /api/sales — duplicate sale number returns 400")]
    public async Task Post_DuplicateNumber_Returns400()
    {
        var num = UniqueNumber();
        await CreateSale(Sale(num));

        var r = await _client.PostAsync("/api/sales", Json(Sale(num)));
        r.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── GET ──────────────────────────────────────────────────────────
    [Fact(DisplayName = "GET /api/sales — returns paginated list")]
    public async Task GetList_ReturnsPaginatedResult()
    {
        await CreateSale(Sale(UniqueNumber()));

        var r = await _client.GetAsync("/api/sales?_page=1&_size=10");
        r.StatusCode.Should().Be(HttpStatusCode.OK);

        var doc = await Doc(r);
        doc.RootElement.GetProperty("totalItems").GetInt32().Should().BeGreaterThan(0);
        doc.RootElement.GetProperty("data").GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact(DisplayName = "GET /api/sales/{id} — returns correct sale")]
    public async Task GetById_ExistingSale_Returns200()
    {
        var (id, _) = await CreateSale(Sale(UniqueNumber()));

        var r = await _client.GetAsync($"/api/sales/{id}");
        r.StatusCode.Should().Be(HttpStatusCode.OK);

        var doc = await Doc(r);
        doc.RootElement.GetProperty("data").GetProperty("id").GetString().Should().Be(id);
    }

    [Fact(DisplayName = "GET /api/sales/{id} — non-existing returns 404")]
    public async Task GetById_NotFound_Returns404()
    {
        var r = await _client.GetAsync("/api/sales/ffffffff-ffff-ffff-ffff-ffffffffffff");
        r.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "GET /api/sales?customerName — wildcard filter works")]
    public async Task GetList_CustomerNameFilter_ReturnsMatchingOnly()
    {
        var payload = new
        {
            saleNumber = UniqueNumber(),
            saleDate = "2026-05-31T00:00:00Z",
            customerId = "aaaa0001-0000-0000-0000-000000000001",
            customerName = "Wildcard User",
            branchId = "bbbb0001-0000-0000-0000-000000000001", branchName = "B",
            items = new[] { new { productId = "cccc0001-0000-0000-0000-000000000001", productName = "P", quantity = 1, unitPrice = 10.0 } }
        };
        await CreateSale(payload);

        var r = await _client.GetAsync("/api/sales?customerName=Wildcard*");
        r.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await Doc(r);
        doc.RootElement.GetProperty("data").EnumerateArray()
            .Should().AllSatisfy(s =>
                s.GetProperty("customerName").GetString().Should().StartWith("Wildcard"));
    }

    // ── UPDATE ────────────────────────────────────────────────────────
    [Fact(DisplayName = "PUT /api/sales/{id} — updates fields and recalculates total")]
    public async Task Put_ValidPayload_UpdatesAndRecalculates()
    {
        var num = UniqueNumber();
        var (id, _) = await CreateSale(Sale(num));

        var update = new
        {
            saleNumber = num,
            saleDate = "2026-06-01T00:00:00Z",
            customerId = "aaaa0001-0000-0000-0000-000000000001",
            customerName = "Updated Name",
            branchId = "bbbb0001-0000-0000-0000-000000000001", branchName = "B",
            items = new[] { new { productId = "cccc0001-0000-0000-0000-000000000001", productName = "P", quantity = 4, unitPrice = 100.0 } }
        };

        var r = await _client.PutAsync($"/api/sales/{id}", Json(update));
        r.StatusCode.Should().Be(HttpStatusCode.OK);

        var doc = await Doc(r);
        doc.RootElement.GetProperty("data").GetProperty("totalAmount").GetDecimal().Should().Be(360m);
        doc.RootElement.GetProperty("data").GetProperty("customerName").GetString().Should().Be("Updated Name");
    }

    // ── CANCEL SALE ───────────────────────────────────────────────────
    [Fact(DisplayName = "PATCH /api/sales/{id}/cancel — cancels active sale")]
    public async Task Cancel_ActiveSale_Returns200Cancelled()
    {
        var (id, _) = await CreateSale(Sale(UniqueNumber()));

        var r = await _client.PatchAsync($"/api/sales/{id}/cancel", null);
        r.StatusCode.Should().Be(HttpStatusCode.OK);

        var doc = await Doc(r);
        doc.RootElement.GetProperty("data").GetProperty("isCancelled").GetBoolean().Should().BeTrue();
    }

    [Fact(DisplayName = "PATCH /api/sales/{id}/cancel — already cancelled returns 400")]
    public async Task Cancel_AlreadyCancelled_Returns400()
    {
        var (id, _) = await CreateSale(Sale(UniqueNumber()));
        await _client.PatchAsync($"/api/sales/{id}/cancel", null);

        var r = await _client.PatchAsync($"/api/sales/{id}/cancel", null);
        r.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── CANCEL ITEM ───────────────────────────────────────────────────
    [Fact(DisplayName = "PATCH /api/sales/{id}/items/{iid}/cancel — cancels item and reduces total")]
    public async Task CancelItem_ValidItem_ReducesTotal()
    {
        var (id, data) = await CreateSale(Sale(UniqueNumber()));
        var originalTotal = data.GetProperty("totalAmount").GetDecimal();
        var itemId = data.GetProperty("items").EnumerateArray().First().GetProperty("id").GetString();

        var r = await _client.PatchAsync($"/api/sales/{id}/items/{itemId}/cancel", null);
        r.StatusCode.Should().Be(HttpStatusCode.OK);

        var after = await _client.GetAsync($"/api/sales/{id}");
        var afterDoc = await Doc(after);
        afterDoc.RootElement.GetProperty("data").GetProperty("totalAmount").GetDecimal()
            .Should().BeLessThan(originalTotal);
    }

    [Fact(DisplayName = "PATCH /api/sales/{id}/items/{iid}/cancel — already cancelled item returns 400")]
    public async Task CancelItem_AlreadyCancelled_Returns400()
    {
        var (id, data) = await CreateSale(Sale(UniqueNumber()));
        var itemId = data.GetProperty("items").EnumerateArray().First().GetProperty("id").GetString();

        await _client.PatchAsync($"/api/sales/{id}/items/{itemId}/cancel", null);
        var r = await _client.PatchAsync($"/api/sales/{id}/items/{itemId}/cancel", null);

        r.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── DELETE ────────────────────────────────────────────────────────
    [Fact(DisplayName = "DELETE /api/sales/{id} — removes sale, subsequent GET returns 404")]
    public async Task Delete_ExistingSale_Returns200ThenNotFound()
    {
        var (id, _) = await CreateSale(Sale(UniqueNumber()));

        var del = await _client.DeleteAsync($"/api/sales/{id}");
        del.StatusCode.Should().Be(HttpStatusCode.OK);

        var get = await _client.GetAsync($"/api/sales/{id}");
        get.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
