using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Contracts.Events
{
    public record ProductCreatedEvent
    {
        public Guid ProductId { get; init; }
        public string Name { get; init; }

        public int StockQuantity { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
