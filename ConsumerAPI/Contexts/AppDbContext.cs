using System;
using ConsumerAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConsumerAPI.Contexts;

public class AppDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

}
