using MenuApi.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace MenuApi.Data
{
    public class MenuDbContext : DbContext
    {
        public MenuDbContext(DbContextOptions<MenuDbContext> options)
            : base(options) { }

        public DbSet<Produto> Produtos { get; set; }
    }
}
