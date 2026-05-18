using MediatR;
using Microsoft.EntityFrameworkCore;
using ProductService.Application.DTOs;
using ProductService.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.GetProducts
{
    public class Handler
    : IRequestHandler<GetProductByIdQuery, ProductDto?>
    {
        private readonly IProductDbContext _context;

        public Handler(IProductDbContext context)
        {
            _context = context;
        }

        public async Task<ProductDto?> Handle(
            GetProductByIdQuery request,
            CancellationToken cancellationToken)
        {
            return await _context.Products
            .Where(x => x.Id == request.Id)
            .Select(x => new ProductDto
            {
                Id = x.Id,
                Name = x.Name,
                Price = x.Price,
                StockQuantity = x.StockQuantity,
                Description = x.Description
            })
            .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
