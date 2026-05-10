using Microsoft.EntityFrameworkCore;
using LRP_Project.Models;

namespace LRP_Project.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Lab> Labs { get; set; }
    public DbSet<Computer> Computers { get; set; }
}