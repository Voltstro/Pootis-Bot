using Microsoft.EntityFrameworkCore;
using Pootis_Bot.Shared.Messages;
using Pootis_Bot.Shared.Models;

namespace Pootis_Bot.Shared;

public class PootisBotDbContext : DbContext
{
    /// <summary>
    ///     Creates a new <see cref="PootisBotDbContext"/> instance
    /// </summary>
    public PootisBotDbContext()
    {
    }
    
    /// <summary>
    ///     Creates a new <see cref="PootisBotDbContext"/> instance
    /// </summary>
    public PootisBotDbContext(DbContextOptions<PootisBotDbContext> options) : base(options)
    {
    }
    
    public DbSet<Profile> Profiles { get; set; }
    
    public DbSet<Server> Servers { get; set; }
    
    public DbSet<ServerMessage> ServerMessages { get; set; }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseNpgsql(x => x.MapEnum<MessageType>())
            .UseSnakeCaseNamingConvention();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //Default Values
        modelBuilder.Entity<Profile>()
            .Property(p => p.CreatedAt)
            .HasDefaultValueSql("now()");
        
        modelBuilder.Entity<Profile>()
            .Property(p => p.UpdatedAt)
            .HasDefaultValueSql("now()");
        
        modelBuilder.Entity<Server>()
            .Property(p => p.CreatedAt)
            .HasDefaultValueSql("now()");
        
        modelBuilder.Entity<Server>()
            .Property(p => p.UpdatedAt)
            .HasDefaultValueSql("now()");
        
        //Indexes
        modelBuilder.Entity<Server>()
            .HasIndex(p => p.DiscordId)
            .IsUnique();
        
        modelBuilder.Entity<Profile>()
            .HasIndex(p => p.DiscordId)
            .IsUnique();
    }
}