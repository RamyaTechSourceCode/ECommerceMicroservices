using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace ProductService.Application.CreateProducts
{
    public record Command(
      string Name,
      string Description,
      decimal Price,
      int StockQuantity
  ) : IRequest<Guid>;
}
