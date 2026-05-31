using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.Persistence
{
    public class OrderDbContext : DbContext
    {
        public OrderDbContext(DbContextOptions<OrderDbContext> options)
            : base(options)
        {
        }

        public DbSet<Order> Orders => Set<Order>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Order>().OwnsMany(x => x.Items);
        }
    }
}
