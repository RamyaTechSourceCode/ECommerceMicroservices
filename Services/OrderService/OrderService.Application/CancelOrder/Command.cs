using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application
{
    public record CancelOrderCommand(Guid OrderId, string Reason)
    : IRequest;
}
