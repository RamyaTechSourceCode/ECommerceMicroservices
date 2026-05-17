using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductService.Api.Requests;
using ProductService.Application.CreateProducts;
using ProductService.Application.Interfaces;
using ProductService.Domain.Entities;

namespace ProductService.Api.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProductsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        //Implementing CQRS [Command Query Responsibility Segregation] 
        // with mediator
        [HttpPost]
        public async Task<IActionResult> Create(CreateProductRequest request)
        {
            var command = new Command(
               request.Name,
               request.Description,
               request.Price,
               request.StockQuantity);

            var id = await _mediator.Send(command);

            return Ok(id);
        }

        /*
        private readonly IProductRepository _repository;

        public ProductsController(IProductRepository repository)
        {
            _repository = repository;
        }

        //Implementing CQRS [Command Query Responsibility Segregation] 
        // without mediator

        [HttpPost]
         public async Task<IActionResult> Create(
         Command command)
         {
            
            var handler = new Handler(_repository);
            var id = await handler.Handle(command);

            return Ok(id);

         }
       
        //No CQRS Implemented

        [HttpPost]
        public async Task<IActionResult> Create(CreateProductRequest request)
        {
            var product = new Product(
                request.Name,
                request.Description,
                request.Price,
                request.StockQuantity);

            await _repository.AddAsync(product);

            await _repository.SaveChangesAsync();

            return Ok();
        }*/

    }
}
