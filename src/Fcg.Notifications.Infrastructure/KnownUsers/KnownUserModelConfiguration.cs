using Fcg.Notifications.Domain.KnownUsers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fcg.Notifications.Infrastructure.KnownUsers;

internal sealed class KnownUserModelConfiguration : IEntityTypeConfiguration<KnownUserModel>
{
    public void Configure(EntityTypeBuilder<KnownUserModel> builder)
    {
        builder.ToTable("known_users");

        builder.HasKey(user => user.Id);
        builder.Property(user => user.Id).HasColumnName("user_id").ValueGeneratedNever();
        builder.Property(user => user.Name).HasMaxLength(KnownUserModel.NameMaxLength).IsRequired();
        builder.Property(user => user.Email).HasMaxLength(KnownUserModel.EmailMaxLength).IsRequired();
        builder.Property(user => user.UpdatedAt).IsRequired();
    }
}
