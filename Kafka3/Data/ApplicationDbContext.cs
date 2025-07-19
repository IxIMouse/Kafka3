using Kafka3.Models;
using Microsoft.EntityFrameworkCore;

namespace Kafka3.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<DataMessage> DataMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DataMessage>().ToTable("DataMessages");
    }
}
