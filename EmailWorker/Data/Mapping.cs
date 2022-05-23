using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace RemesasAPI.Entities;


class BaseEntityConfiguration<T> : IEntityTypeConfiguration<T> where T : BaseEntity
{
    protected readonly int balIntPrec = 18;
    protected readonly int balDecPrec = 2;

    virtual public void Configure(EntityTypeBuilder<T> builder)
    {
        builder.Property(u => u.Id).HasColumnName("id").IsRequired();
        builder.Property(u => u.StrongId).HasColumnName("strong_id").IsRequired();
        builder.Property(u => u.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(u => u.UpdatedAt).HasColumnName("updated_at").IsRequired();

        builder.HasKey(u => u.Id);
        builder.HasIndex(u => u.StrongId).IsUnique();
    }
}

class EmailQueueMapping : BaseEntityConfiguration<EmailQueue>
{
    override public void Configure(EntityTypeBuilder<EmailQueue> builder)
    {
        builder.ToTable("send_email");

        base.Configure(builder);

        builder.Property(e => e.ToEmail).HasColumnName("to_email").IsRequired();
        builder.Property(e => e.Subject).HasColumnName("subject").IsRequired();
        builder.Property(e => e.SentDate).HasColumnName("send_date");
        builder.Property(e => e.Body).HasColumnName("body").IsRequired();
    }
}