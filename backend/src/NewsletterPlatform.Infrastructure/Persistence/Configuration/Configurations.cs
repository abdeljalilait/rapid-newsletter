using NewsletterPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace NewsletterPlatform.Infrastructure.Persistence.Configuration;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email).HasMaxLength(256).IsRequired();
        builder.HasIndex(u => u.Email).IsUnique();

        builder.Property(u => u.PasswordHash).HasMaxLength(256).IsRequired();
        builder.Property(u => u.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(u => u.LastName).HasMaxLength(100);
        builder.Property(u => u.EmailConfirmedAt);
        builder.Property(u => u.PasswordResetTokenHash).HasMaxLength(128);
        builder.Property(u => u.PasswordResetTokenExpiresAt);
        builder.Property(u => u.EmailConfirmationTokenHash).HasMaxLength(128);
        builder.Property(u => u.EmailConfirmationTokenExpiresAt);
        builder.Property(u => u.CreatedAt).IsRequired();
        builder.Property(u => u.UpdatedAt).IsRequired();

        builder.HasMany(u => u.RefreshTokens)
            .WithOne()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

    }
}

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.TokenHash).HasMaxLength(128).IsRequired();
        builder.HasIndex(t => t.TokenHash).IsUnique();
        builder.HasIndex(t => t.UserId);
        builder.Property(t => t.ReplacesTokenId);
        builder.Property(t => t.ExpiresAt).IsRequired();
        builder.Property(t => t.RevokedAt);
        builder.Property(t => t.RevokedReason).HasMaxLength(256);
        builder.Property(t => t.CreatedByIp).HasMaxLength(64);
        builder.Property(t => t.Status).HasConversion<int>().IsRequired();
        builder.Property(t => t.CreatedAt).IsRequired();
        builder.Property(t => t.UpdatedAt).IsRequired();
    }
}

public class WorkspaceConfiguration : IEntityTypeConfiguration<Workspace>
{
    public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Workspace> builder)
    {
        builder.ToTable("workspaces");
        builder.HasKey(w => w.Id);

        builder.Property(w => w.Name).HasMaxLength(120).IsRequired();
        builder.Property(w => w.Slug).HasMaxLength(63).IsRequired();
        builder.HasIndex(w => w.Slug).IsUnique();
        builder.Property(w => w.LogoUrl).HasMaxLength(1024);
        builder.Property(w => w.Description).HasMaxLength(2000);
        builder.Property(w => w.DefaultSenderName).HasMaxLength(120).IsRequired();
        builder.Property(w => w.DefaultSenderEmail).HasMaxLength(256).IsRequired();
        builder.Property(w => w.Timezone).HasMaxLength(64).IsRequired();
        builder.Property(w => w.DefaultCurrency).HasMaxLength(3).IsRequired();
        builder.Property(w => w.Status).HasConversion<int>().IsRequired();
        builder.Property(w => w.OwnerId).IsRequired();
        builder.Property(w => w.CreatedAt).IsRequired();
        builder.Property(w => w.UpdatedAt).IsRequired();

        builder.HasMany(w => w.Members)
            .WithOne()
            .HasForeignKey(m => m.WorkspaceId)
            .OnDelete(DeleteBehavior.Cascade);

    }
}

public class WorkspaceMemberConfiguration : IEntityTypeConfiguration<WorkspaceMember>
{
    public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<WorkspaceMember> builder)
    {
        builder.ToTable("workspace_members");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.WorkspaceId).IsRequired();
        builder.Property(m => m.UserId).IsRequired();
        builder.Property(m => m.Role).HasConversion<int>().IsRequired();
        builder.Property(m => m.InvitedByEmail).HasMaxLength(256);
        builder.Property(m => m.JoinedAt);
        builder.Property(m => m.CreatedAt).IsRequired();
        builder.Property(m => m.UpdatedAt).IsRequired();

        builder.HasIndex(m => new { m.WorkspaceId, m.UserId }).IsUnique();
        builder.HasIndex(m => m.UserId);
    }
}

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.WorkspaceId);
        builder.Property(a => a.ActorUserId);
        builder.Property(a => a.Action).HasMaxLength(100).IsRequired();
        builder.Property(a => a.EntityType).HasMaxLength(100);
        builder.Property(a => a.EntityId);
        builder.Property(a => a.Metadata).HasColumnType("jsonb");
        builder.Property(a => a.IpAddress).HasMaxLength(64);
        builder.Property(a => a.CreatedAt).IsRequired();

        builder.HasIndex(a => a.WorkspaceId);
        builder.HasIndex(a => a.ActorUserId);
    }
}
