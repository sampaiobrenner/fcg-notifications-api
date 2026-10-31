using Fcg.Notifications.Domain.EmailLogs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fcg.Notifications.Infrastructure.EmailLogs;

internal sealed class EmailLogModelConfiguration : IEntityTypeConfiguration<EmailLogModel>
{
    public void Configure(EntityTypeBuilder<EmailLogModel> builder)
    {
        builder.ToTable("email_log");

        builder.HasKey(log => log.Id);
        builder.Property(log => log.Id).ValueGeneratedNever();
        builder.Property(log => log.EventId).IsRequired();
        builder.HasIndex(log => log.EventId).IsUnique();
        builder.Property(log => log.Kind).HasConversion<string>().HasMaxLength(EmailLogModel.KindMaxLength).IsRequired();
        builder.Property(log => log.ToEmail).HasMaxLength(EmailLogModel.ToEmailMaxLength).IsRequired();
        builder.Property(log => log.Subject).HasMaxLength(EmailLogModel.SubjectMaxLength).IsRequired();
        builder.Property(log => log.SentAt).IsRequired();
    }
}
