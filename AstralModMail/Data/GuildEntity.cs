using System.ComponentModel.DataAnnotations;

namespace AstralModMail.Data;

public class GuildEntity
{
    [Key]
    public ulong GuildId { get; set; }
    public ulong Category { get; set; }
    public ulong Log { get; set; }
}