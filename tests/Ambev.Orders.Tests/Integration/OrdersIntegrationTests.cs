using System.Net;
using System.Net.Http.Json;
using Bogus;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Orders.API.Requests;
using Orders.API.Responses;

namespace Orders.Tests.Integration;

public class OrdersIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly Faker _faker = new("pt_BR");

    public OrdersIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    private CreateOrderRequest BuildRequest(int? orderId = null) => new(
        OrderId: orderId ?? _faker.Random.Int(1, 99999),
        ClientId: _faker.Random.Int(1, 99999),
        Items:
        [
            new OrderItemRequest(
                ProductId: _faker.Random.Int(1, 9999),
                Quantity: _faker.Random.Int(1, 10),
                Value: _faker.Finance.Amount(1, 500))
        ]);

    [Fact]
    public async Task POST_CreateOrder_ValidRequest_Returns201WithIdAndStatus()
    {
        var request = BuildRequest();

        var response = await _client.PostAsJsonAsync("/api/orders", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<CreateOrderResponse>();
        body.Should().NotBeNull();
        body!.Id.Should().BeGreaterThan(0);
        body.Status.Should().Be("Created");
    }

    [Fact]
    public async Task POST_CreateOrder_DuplicateOrderId_Returns409()
    {
        var request = BuildRequest();

        await _client.PostAsJsonAsync("/api/orders", request);
        var response = await _client.PostAsJsonAsync("/api/orders", request);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task POST_CreateOrder_InvalidOrderId_Returns400()
    {
        var request = new CreateOrderRequest(
            OrderId: 0,
            ClientId: 1,
            Items: [new OrderItemRequest(1, 1, 10)]);

        var response = await _client.PostAsJsonAsync("/api/orders", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GET_GetById_ExistingOrder_Returns200WithFullOrder()
    {
        var request = BuildRequest();
        var created = await _client.PostAsJsonAsync("/api/orders", request);
        var createdBody = await created.Content.ReadFromJsonAsync<CreateOrderResponse>();

        var response = await _client.GetAsync($"/api/orders/{createdBody!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<OrderResponse>();
        body.Should().NotBeNull();
        body!.OrderId.Should().Be(request.OrderId);
        body.ClientId.Should().Be(request.ClientId);
        body.Tax.Should().BeGreaterThan(0);
        body.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task GET_GetById_NonExistentOrder_Returns404()
    {
        var response = await _client.GetAsync("/api/orders/999999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GET_ListByStatus_Created_ReturnsMatchingOrders()
    {
        var request = BuildRequest();
        await _client.PostAsJsonAsync("/api/orders", request);

        var response = await _client.GetAsync("/api/orders?status=Created");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<IEnumerable<OrderResponse>>();
        body.Should().NotBeNullOrEmpty();
        body!.Should().OnlyContain(o => o.Status == "Created");
    }

    [Fact]
    public async Task GET_ListByStatus_InvalidStatus_Returns400()
    {
        var response = await _client.GetAsync("/api/orders?status=Invalid");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── PATCH /api/orders/{id}/start-processing ────────────────────────────────

    [Fact]
    public async Task PATCH_StartProcessing_CreatedOrder_Returns200WithProcessingStatus()
    {
        var request = BuildRequest();
        var created = await _client.PostAsJsonAsync("/api/orders", request);
        var createdBody = await created.Content.ReadFromJsonAsync<CreateOrderResponse>();

        var response = await _client.PatchAsync($"/api/orders/{createdBody!.Id}/start-processing", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<OrderResponse>();
        body!.Status.Should().Be("Processing");
    }

    [Fact]
    public async Task PATCH_StartProcessing_AlreadyProcessingOrder_Returns400()
    {
        var request = BuildRequest();
        var created = await _client.PostAsJsonAsync("/api/orders", request);
        var createdBody = await created.Content.ReadFromJsonAsync<CreateOrderResponse>();
        var id = createdBody!.Id;

        await _client.PatchAsync($"/api/orders/{id}/start-processing", null);
        var response = await _client.PatchAsync($"/api/orders/{id}/start-processing", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PATCH_StartProcessing_NonExistentOrder_Returns404()
    {
        var response = await _client.PatchAsync("/api/orders/999999/start-processing", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PATCH_Send_ProcessingOrder_Returns200WithSentStatus()
    {
        var request = BuildRequest();
        var created = await _client.PostAsJsonAsync("/api/orders", request);
        var createdBody = await created.Content.ReadFromJsonAsync<CreateOrderResponse>();
        var id = createdBody!.Id;

        await _client.PatchAsync($"/api/orders/{id}/start-processing", null);
        var response = await _client.PatchAsync($"/api/orders/{id}/send", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<OrderResponse>();
        body!.Status.Should().Be("Sent");
    }

    [Fact]
    public async Task PATCH_Send_CreatedOrder_Returns400()
    {
        var request = BuildRequest();
        var created = await _client.PostAsJsonAsync("/api/orders", request);
        var createdBody = await created.Content.ReadFromJsonAsync<CreateOrderResponse>();

        var response = await _client.PatchAsync($"/api/orders/{createdBody!.Id}/send", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PATCH_Send_NonExistentOrder_Returns404()
    {
        var response = await _client.PatchAsync("/api/orders/999999/send", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
