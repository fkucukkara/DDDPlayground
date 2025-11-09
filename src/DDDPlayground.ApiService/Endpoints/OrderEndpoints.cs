using DDDPlayground.ApiService.Models;
using DDDPlayground.Application.Orders;
using DDDPlayground.Application.Orders.ConfirmOrder;
using DDDPlayground.Application.Orders.CreateOrder;
using DDDPlayground.Application.Orders.GetOrder;
using DDDPlayground.Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace DDDPlayground.ApiService.Endpoints;

/// <summary>
/// Minimal API endpoints for Order aggregate.
/// Demonstrates DDD + CQRS + Minimal API best practices.
/// 
/// Key Patterns:
/// - MapGroup for endpoint organization
/// - TypedResults for type-safe responses
/// - Static handler methods for better performance
/// - Proper HTTP status codes (201 Created, 404 Not Found, etc.)
/// - WithTags and WithOpenApi for Swagger documentation
/// </summary>
public static class OrderEndpoints
{
    public static RouteGroupBuilder MapOrderEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/orders")
            .WithTags("Orders")
            .WithOpenApi();

        group.MapPost("/", CreateOrderAsync)
            .WithName("CreateOrder")
            .WithSummary("Create a new order")
            .Produces<OrderResponse>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapGet("/{id:guid}", GetOrderAsync)
            .WithName("GetOrder")
            .WithSummary("Get an order by ID")
            .Produces<OrderResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/confirm", ConfirmOrderAsync)
            .WithName("ConfirmOrder")
            .WithSummary("Confirm an order")
            .Produces<OrderResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        return group;
    }

    /// <summary>
    /// Create a new order.
    /// Static handler method for better performance (avoid closure allocations).
    /// </summary>
    private static async Task<Results<Created<OrderResponse>, BadRequest<ProblemDetails>>> CreateOrderAsync(
        CreateOrderRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new CreateOrderCommand
            {
                CustomerId = request.CustomerId,
                Items = request.Items.Select(i => new CreateOrderItemDto
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    Currency = i.Currency
                }).ToList()
            };

            var response = await sender.Send(command, cancellationToken);

            return TypedResults.Created($"/api/orders/{response.Id}", response);
        }
        catch (DomainException ex)
        {
            return TypedResults.BadRequest(new ProblemDetails
            {
                Title = "Domain validation failed",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }

    /// <summary>
    /// Get an order by ID.
    /// </summary>
    private static async Task<Results<Ok<OrderResponse>, NotFound<ProblemDetails>>> GetOrderAsync(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new GetOrderQuery(id);
        var response = await sender.Send(query, cancellationToken);

        return response is null
            ? TypedResults.NotFound(new ProblemDetails
            {
                Title = "Order not found",
                Detail = $"Order with ID {id} was not found",
                Status = StatusCodes.Status404NotFound
            })
            : TypedResults.Ok(response);
    }

    /// <summary>
    /// Confirm an order (change status from Draft to Confirmed).
    /// </summary>
    private static async Task<Results<Ok<OrderResponse>, NotFound<ProblemDetails>, BadRequest<ProblemDetails>>> ConfirmOrderAsync(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new ConfirmOrderCommand(id);
            var response = await sender.Send(command, cancellationToken);

            return response is null
                ? TypedResults.NotFound(new ProblemDetails
                {
                    Title = "Order not found",
                    Detail = $"Order with ID {id} was not found",
                    Status = StatusCodes.Status404NotFound
                })
                : TypedResults.Ok(response);
        }
        catch (DomainException ex)
        {
            return TypedResults.BadRequest(new ProblemDetails
            {
                Title = "Domain validation failed",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }
}
