namespace RemesasAPI.Entities;
public class BaseEntity
{
    public int Id { get; set; }
    public Guid StrongId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class BaseEntityDto
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public BaseEntityDto(BaseEntity entity)
    {
        Id = entity.StrongId;
        CreatedAt = entity.CreatedAt;
        UpdatedAt = entity.CreatedAt;
    }
}