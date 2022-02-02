using System.ComponentModel.DataAnnotations;

namespace ModMail.Data.Entities;

public class AvatarEntity
{
    [Key] public ulong MessageId { get; set; }
    public byte[] Data { get; set; } = null!;
    public MessageEntity Message { get; set; } = null!;
}