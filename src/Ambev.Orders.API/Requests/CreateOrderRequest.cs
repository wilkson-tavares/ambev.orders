namespace Orders.API.Requests;

public record CreateOrderRequest(
    int OrderId,
    int ClientId,
    IEnumerable<OrderItemRequest> Items);

public record OrderItemRequest(
    int ProductId,
    int Quantity,
    decimal Value);