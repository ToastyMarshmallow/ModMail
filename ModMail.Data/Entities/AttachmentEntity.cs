using System.ComponentModel.DataAnnotations;

namespace ModMail.Data.Entities;

public class AttachmentEntity
{
    [Key] public string Name { get; set; } = null!;
    public byte[] Data { get; set; } = null!;
    public ulong MessageId { get; set; }
    public MessageEntity MessageEntity { get; set; } = null!;
}