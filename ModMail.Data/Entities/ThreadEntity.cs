using System.ComponentModel.DataAnnotations;

namespace ModMail.Data.Entities;

public class ThreadEntity
{
    [Key]
    public ulong Channel { get; set; }
    public ulong Recipient { get; set; }
    public ulong Guild { get; set; }
    public GuildEntity GuildEntity { get; set; } = null!;
    public List<MessageEntity> MessageEntities { get; set; } = new();
}