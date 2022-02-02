using System.ComponentModel.DataAnnotations;

namespace ModMail.Data.Entities;

public class GuildEntity
{
    [Key]
    public ulong GuildId { get; set; }
    public ulong Category { get; set; }
    public ulong Log { get; set; }
    public List<ThreadEntity> ThreadEntities { get; set; } = null!;
}