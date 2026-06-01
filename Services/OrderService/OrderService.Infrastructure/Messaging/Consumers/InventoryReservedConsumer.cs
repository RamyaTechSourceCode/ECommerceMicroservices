using ECommerce.Contracts.Events;
using MassTransit;
using MediatR;
using OrderService.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.Messaging.Consumers
{
    public class InventoryReservedConsumer : IConsumer<InventoryReservedEvent>
    {
        private readonly IMediator _mediator;

        public InventoryReservedConsumer(IMediator mediator)
        {
            _mediator = mediator;
        }

        public Task Consume(ConsumeContext<InventoryReservedEvent> context)
        {
            return _mediator.Send(new ConfirmOrderCommand(context.Message.OrderId));
        }
    }
}
