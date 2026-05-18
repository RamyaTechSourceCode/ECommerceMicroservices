using FluentValidation;
using MediatR;
using ProductService.Application.Interfaces;
using ProductService.Domain.Entities;

namespace ProductService.Application.CreateProducts
{
    public class Handler
     : IRequestHandler<Command, Guid> // for MediatR
    {
        //hander -> dbcontext
        private readonly IProductDbContext _context;

        public Handler(IProductDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            var product = new Product(
                request.Name,
                request.Description,
                request.Price,
                request.StockQuantity);

            _context.Products.Add(product);

            await _context.SaveChangesAsync(cancellationToken);

            return product.Id;
        }
        /*
        // handler -> repository-> dbcontext
        private readonly IProductRepository _repository;

        public Handler(IProductRepository repository)
        {
            _repository = repository;
        }

        public async Task<Guid> Handle(
            Command request,
            CancellationToken cancellationToken)// Cancellationtoken for MediatR
        {
            var product = new Product(
                request.Name,
                request.Description,
                request.Price,
                request.StockQuantity);

            await _repository.AddAsync(product);

            return product.Id;
        }
        */
        /*
        //Manual validation without a pipeline [validator.cs] when using MediatR  
        private readonly IValidator<Command> _validator;

        public Handler(IValidator<Command> validator)
        {
            _validator = validator;
        }

        public async Task<Guid> Handle(
            Command request,
            CancellationToken cancellationToken)// Cancellationtoken for MediatR
        {

            var result = await _validator.ValidateAsync(request);

            if (!result.IsValid)
            {
                throw new ValidationException(result.Errors);
            }

            return new Guid();
        }
        */
    }
}
