using MediatR;
using OrderService.Domain.Entities;
using OrderService.Infrastructure.Messaging.Kafka.Producers;
using OrderService.Infrastructure.Persistence;
using Shared.Contracts.Orders.Kafka;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.CreateOrder
{
    public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, Guid>
    {
        private readonly OrderDbContext _db;
        //private readonly IEventBus _bus;
        private readonly OrderCreatedProducer _producer;

        public CreateOrderHandler(
            OrderDbContext db,// IEventBus bus)
            OrderCreatedProducer producer)
        {
            _db = db;
            //_bus = bus;
            _producer = producer;
        }

        public async Task<Guid> Handle(
            CreateOrderCommand request,
            CancellationToken cancellationToken)
        {
            var order = new Domain.Entities.Order
            {
                Id = Guid.NewGuid(),
                CustomerId = request.CustomerId,
                Items = request.Items.Select(x => new OrderItem
                {
                    ProductId = x.ProductId,
                    Quantity = x.Quantity,
                    Price = x.Price
                }).ToList(),
                TotalAmount = request.Items.Sum(x => x.Price * x.Quantity)
            };

            _db.Orders.Add(order);

            await _db.SaveChangesAsync(cancellationToken);

           /* await _bus.PublishAsync("order.created",
                new OrderCreatedEvent
                {
                    OrderId = order.Id,
                    Items = request.Items.Select(x => new OrderItemEvent
                    {
                        ProductId = x.ProductId,
                        Quantity = x.Quantity
                    }).ToList()
                });*/

            var orderCreated = new OrderCreatedEvent
            {
                OrderId = order.Id,
                CustomerId = order.CustomerId,
                TotalAmount = order.TotalAmount,
                CreatedAt = order.CreatedAt
            };

            await _producer.PublishAsync(
                "orders.created",
                order.Id.ToString(),
                orderCreated);

            return order.Id;
        }
    }
}
