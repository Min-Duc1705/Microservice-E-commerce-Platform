using AuthService.Models;
using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Data;

public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

    public DbSet<AppUser> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    
    // ── MassTransit Outbox Tables ──────────────────────────────────────────────
    // Bảng lưu tạm Event chưa bắn được lên RabbitMQ (Outbox)
    public DbSet<OutboxMessage> OutboxMessages { get; set; }
    // Bảng theo dõi trạng thái Outbox của từng Task
    public DbSet<OutboxState> OutboxStates { get; set; }
    // Bảng đảm bảo Idempotency phía Consumer Service (tránh nhận Event 2 lần)
    public DbSet<InboxState> InboxStates { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // AppUser — N-1 với Role
        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique().HasDatabaseName("IX_Users_Email");
            entity.HasIndex(e => e.Username).HasDatabaseName("IX_Users_Username");
            entity.HasIndex(e => e.RoleId).HasDatabaseName("IX_Users_RoleId");

            entity.HasOne(u => u.Role)
                  .WithMany(r => r.Users)
                  .HasForeignKey(u => u.RoleId)
                  .OnDelete(DeleteBehavior.SetNull); // Xóa Role không xóa User
        });

        // Role — M-N với Permission (join table tự động)
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique().HasDatabaseName("IX_Roles_Name");

            entity.HasMany(r => r.Permissions)
                  .WithMany(p => p.Roles)
                  .UsingEntity(j => j.ToTable("RolePermissions")); // Bảng join
        });

        // Permission
        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.Id);
            // Composite unique: cùng path + method chỉ tồn tại 1 lần
            entity.HasIndex(e => new { e.ApiPath, e.Method })
                  .IsUnique()
                  .HasDatabaseName("IX_Permissions_Path_Method");
        });
        
        // ── Map MassTransit Outbox Tables ──────────────────────────────────────
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }
}
