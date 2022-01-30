using System.ComponentModel.DataAnnotations;

namespace AstralModMail.Data;

public class ThreadEntity
{
    [Key]
    public ulong Channel { get; set; }
    public ulong Recipient { get; set; }
    public ulong Guild { get; set; }
}