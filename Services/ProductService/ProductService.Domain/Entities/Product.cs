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

        public int StockQuantity { get;  set; }

        public DateTime CreatedAt { get;  set; }

        public Product(
            string name,
            string description,
            decimal price,
            int stockQuantity)
        {
            Id = Guid.NewGuid();
            Name = name;
            Description = description;
            Price = price;
            StockQuantity = stockQuantity;
            CreatedAt = DateTime.UtcNow;
        }

        public void Update(
            string name,
            string description,
            decimal price,
            int stockQuantity)
        {
            Name = name;
            Description = description;
            Price = price;
            StockQuantity = stockQuantity;
        }
    }
}
