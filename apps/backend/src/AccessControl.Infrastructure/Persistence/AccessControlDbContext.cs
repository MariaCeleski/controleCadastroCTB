using AccessControl.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AccessControl.Infrastructure.Persistence;

public sealed class AccessControlDbContext(DbContextOptions<AccessControlDbContext> options) : DbContext(options)
{
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<AccessCredential> Credentials => Set<AccessCredential>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Company>(entity =>
        {
            entity.ToTable("companies");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.CompanyName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.CnpjDigits).HasMaxLength(14).IsRequired();
            entity.HasIndex(x => new { x.OrganizationId, x.CnpjDigits }).IsUnique();
            entity.HasMany<AccessCredential>().WithOne().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<AccessCredential>(entity =>
        {
            entity.ToTable("access_credentials");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.EncryptedPassword).IsRequired();
        });
    }
}
