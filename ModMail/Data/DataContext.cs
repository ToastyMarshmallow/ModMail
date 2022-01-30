using Microsoft.EntityFrameworkCore;

namespace ModMail.Data;

public class DataContext : DbContext
{
    public DbSet<GuildEntity> Guilds { get; set; } = null!;
    public DbSet<ThreadEntity> Threads { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql("Host=localhost; Database=Modmail; Username=toasty; Password=toasty;");
    }
}