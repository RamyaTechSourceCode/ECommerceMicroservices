using FluentValidation;
using MediatR;
using ProductService.Application.Interfaces;
using ProductService.Domain.Entities;

namespace ProductService.Application.CreateProducts
{
    public class Handler
     : IRequestHandler<Command, Guid> // for MediatR
    {
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
        /*
        //Manual validation without a pipeline when using MediatR  
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
