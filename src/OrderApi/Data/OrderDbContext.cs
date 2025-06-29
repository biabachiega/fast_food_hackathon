using Microsoft.EntityFrameworkCore;
using OrderApi.Entities;
using System.Collections.Generic;

namespace OrderApi.Data
{
    public class OrderDbContext : DbContext
    {
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<ItemPedido> ItemPedido { get; set; }
        public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Pedido>()
                .Property(p => p.Status)
                .HasConversion<string>();

            modelBuilder.Entity<Pedido>()
                .Property(p => p.FormaEntrega)
                .HasConversion<string>();
        }
    }
}
