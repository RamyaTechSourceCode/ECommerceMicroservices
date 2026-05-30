using MediatR;
using ProductService.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.DeleteProduct
{
    public class DeleteProductHandler
    : IRequestHandler<DeleteProductCommand, bool>
    {
        private readonly IProductDbContext _context;

        public DeleteProductHandler(IProductDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(
            DeleteProductCommand request,
            CancellationToken cancellationToken)
        {
            var product = await _context.Products
                .FindAsync(request.Id);

            if (product == null)
                return false;

            _context.Products.Remove(product);

            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
