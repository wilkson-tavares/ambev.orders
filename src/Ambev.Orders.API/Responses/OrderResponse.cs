using Orders.Domain.Entities;

namespace Orders.API.Responses;

public record OrderResponse(
    int Id,
    int OrderId,
    int ClientId,
    decimal Tax,
    string Status,
    IEnumerable<OrderItemResponse> Items)
{
    public static OrderResponse FromEntity(Order order) => new(
        order.Id,
        order.OrderId,
        order.ClientId,
        order.Tax,
        order.Status.ToString(),
        order.Items.Select(i => new OrderItemResponse(
            i.ProductId,
            i.Quantity,
            i.Value)));
}

public record OrderItemResponse(
    int ProductId,
    int Quantity,
    decimal Value);

public record CreateOrderResponse(int Id, string Status);