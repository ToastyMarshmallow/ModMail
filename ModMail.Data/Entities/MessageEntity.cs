using System.ComponentModel.DataAnnotations;

namespace ModMail.Data.Entities;

public enum MessageType
{
    Internal,
    Reply,
    Webhook
}

public class MessageEntity
{
    [Key] public ulong MessageId { get; set; }
    public bool Anonymous { get; set; }
    public MessageType Type { get; set; }
    public DateTime CreatedAt { get; set; }
    public ulong ThreadId { get; set; }
    public ThreadEntity ThreadEntity { get; set; } = null!;
    public string Author { get; set; } = null!;
    public AvatarEntity AuthorAvatar { get; set; } = null!;
    public string Content { get; set; } = null!;
    public List<AttachmentEntity> Attachments { get; set; } = null!;
}