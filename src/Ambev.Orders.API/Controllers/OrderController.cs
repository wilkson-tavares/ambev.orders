using Microsoft.AspNetCore.Mvc;
using Orders.API.Requests;
using Orders.API.Responses;
using Orders.Domain.Entities;
using Orders.Domain.Enums;
using Orders.Domain.Interfaces.Services;

namespace Orders.API.Controllers;

[ApiController]
[Route("api/orders")]
public sealed class OrdersController : ControllerBase
{
    private readonly IOrderService _service;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IOrderService service, ILogger<OrdersController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(typeof(CreateOrderResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateOrderRequest request,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "Receiving order {OrderId} from customer {ClientId}",
            request.OrderId, request.ClientId);

        var items = request.Items.Select(i =>
            new OrderItem(i.ProductId, i.Quantity, i.Value));

        var order = await _service.CreateAsync(
            request.OrderId, request.ClientId, items, ct);

        _logger.LogInformation(
            "Order {OrderId} created successfully. Internal Id: {Id}",
            order.OrderId, order.Id);

        var response = new CreateOrderResponse(order.Id, order.Status.ToString());

        return CreatedAtAction(nameof(GetById), new { id = order.Id }, response);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var order = await _service.GetByIdAsync(id, ct);
        return Ok(OrderResponse.FromEntity(order));
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OrderResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ListByStatus(
        [FromQuery] string status,
        CancellationToken ct)
    {
        if (!Enum.TryParse<OrderStatus>(status, ignoreCase: true, out var statusEnum))
            return BadRequest($"Invalid status '{status}'. Accepted values: Created, Processing, Sent.");

        var orders = await _service.ListByStatusAsync(statusEnum, ct);

        return Ok(orders.Select(OrderResponse.FromEntity));
    }

    [HttpPatch("{id:int}/start-processing")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> StartProcessing(int id, CancellationToken ct)
    {
        var order = await _service.StartProcessingAsync(id, ct);
        return Ok(OrderResponse.FromEntity(order));
    }

    [HttpPatch("{id:int}/send")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Send(int id, CancellationToken ct)
    {
        var order = await _service.SendAsync(id, ct);
        return Ok(OrderResponse.FromEntity(order));
    }
}
