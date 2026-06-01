using MediatR;
using Microsoft.EntityFrameworkCore;
using ProductService.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.UpdateProducts
{
    public class UpdateProductHandler
      : IRequestHandler<UpdateProductCommand, bool>
    {
        private readonly IProductDbContext _context;

        public UpdateProductHandler(IProductDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(
            UpdateProductCommand request,
            CancellationToken cancellationToken)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(
                    x => x.Id == request.Id,
                    cancellationToken);

            if (product is null)
                return false;

            product.Update(
                request.Name,
                request.Description,
                request.Price);

            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
