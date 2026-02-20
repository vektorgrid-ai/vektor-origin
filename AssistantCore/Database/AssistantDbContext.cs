using AssistantCore.Companion;
using AssistantCore.Companion.Dto;
using Microsoft.EntityFrameworkCore;

namespace AssistantCore.Database;

public class AssistantDbContext : DbContext
{
    public AssistantDbContext(DbContextOptions<AssistantDbContext> opts) : base(opts) { }

    public DbSet<CompanionDevice> Companions { get; set; }
    public DbSet<ToolApprovalRequest> ToolApprovals { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Map Permissions to the device. This stores the permissions in the same table as the device
        modelBuilder.Entity<CompanionDevice>().OwnsOne(d => d.Permissions);
        modelBuilder.Entity<ToolApprovalRequest>().OwnsOne(d => d.Tool);
    }
}
