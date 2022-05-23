namespace RemesasAPI.Entities;

public class EmailQueue: BaseEntity
{
    public string ToEmail { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
    public DateTime? SentDate { get; set; }
}

