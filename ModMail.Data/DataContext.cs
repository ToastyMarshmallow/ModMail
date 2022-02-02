using Microsoft.EntityFrameworkCore;
using ModMail.Data.Entities;

namespace ModMail.Data;

public class DataContext : DbContext
{
    public DbSet<GuildEntity> Guilds { get; set; } = null!;
    public DbSet<ThreadEntity> Threads { get; set; } = null!;
    public DbSet<MessageEntity> Messages { get; set; } = null!;
    public DbSet<AttachmentEntity> Attachments { get; set; } = null!;
    public DbSet<AvatarEntity> Avatars { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql("Host=localhost; Database=Modmail; Username=toasty; Password=toasty;");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GuildEntity>()
            .HasMany(x => x.ThreadEntities)
            .WithOne(x => x.GuildEntity)
            .HasForeignKey(x => x.Guild);
        modelBuilder.Entity<ThreadEntity>()
            .HasMany(x => x.MessageEntities)
            .WithOne(x => x.ThreadEntity)
            .HasForeignKey(x => x.ThreadId);
        modelBuilder.Entity<MessageEntity>()
            .HasMany(x => x.Attachments)
            .WithOne(x => x.MessageEntity)
            .HasForeignKey(x => x.MessageId);
        modelBuilder.Entity<MessageEntity>()
            .HasOne(x => x.AuthorAvatar)
            .WithOne(x => x.Message);
    }
}