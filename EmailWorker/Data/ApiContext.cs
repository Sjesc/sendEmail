
namespace RemesasAPI;

using Microsoft.EntityFrameworkCore;
using RemesasAPI.Entities;

public class ApiContext : DbContext
{
    public ApiContext()
    { }

    public ApiContext(DbContextOptions<ApiContext> options)
       : base(options)
    {
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer($"Server=172.16.12.31;Database=UnidigitalRemesasV4.0.0;User Id=developer;Password=unico2014;", options =>
        {
            options.EnableRetryOnFailure();
        });
    }
    
    public DbSet<EmailQueue> EmailQueue { get; set; } = null!;

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var insertedEntries = this.ChangeTracker.Entries()
            .Where(x => x.State == EntityState.Added)
            .Select(x => x.Entity);

        foreach (var insertedEntry in insertedEntries)
        {
            if (insertedEntry is BaseEntity baseEntity)
            {
                baseEntity.StrongId = Guid.NewGuid();
                baseEntity.CreatedAt = DateTime.UtcNow;
                baseEntity.UpdatedAt = DateTime.UtcNow;
            }
        }

        var modifiedEntries = this.ChangeTracker.Entries()
            .Where(x => x.State == EntityState.Modified)
            .Select(x => x.Entity);

        foreach (var modifiedEntry in modifiedEntries)
        {
            if (modifiedEntry is BaseEntity baseEntity)
            {
                baseEntity.UpdatedAt = DateTime.UtcNow;
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new EmailQueueMapping());

    }
}


