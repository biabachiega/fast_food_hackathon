using Microsoft.EntityFrameworkCore;
using OrderApi.Entities;
using System.Collections.Generic;

namespace OrderApi.Data
{
    public class OrderDbContext : DbContext
    {
        public DbSet<Pedido> Pedidos { get; set; }
        public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options) { }

    }
}
