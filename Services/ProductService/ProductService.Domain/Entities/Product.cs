using System;
using System.Collections.Generic;
using System.Text;

namespace ProductService.Domain.Entities
{
    public class Product
    {
        public Guid Id { get;  set; }

        public string Name { get;  set; }

        public string Description { get;  set; }

        public decimal Price { get;  set; }

       
        public DateTime CreatedAt { get;  set; }

        public Product(
            string name,
            string description,
            decimal price)
        {
            Id = Guid.NewGuid();
            Name = name;
            Description = description;
            Price = price;
            CreatedAt = DateTime.UtcNow;
        }

        public void Update(
            string name,
            string description,
            decimal price)
        {
            Name = name;
            Description = description;
            Price = price;
        }
    }
}
