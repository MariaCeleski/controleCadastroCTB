using AccessControl.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AccessControl.Infrastructure.Persistence;

public sealed class AccessControlDbContext(DbContextOptions<AccessControlDbContext> options) : DbContext(options)
{
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<AccessCredential> Credentials => Set<AccessCredential>();
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Organization>(entity =>
        {
            entity.ToTable("organizations");
            entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
        });
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.Property(x => x.Email).HasMaxLength(254).IsRequired();
            entity.Property(x => x.PasswordHash).IsRequired();
            entity.HasIndex(x => x.Email).IsUnique();
        });
        modelBuilder.Entity<Company>(entity =>
        {
            entity.ToTable("companies");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.CompanyName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.CnpjDigits).HasMaxLength(14).IsRequired();
            // A CNPJ is unique only inside the current customer organization, never globally across clients.
            entity.HasIndex(x => new { x.OrganizationId, x.CnpjDigits }).IsUnique();
            // Credentials are persisted explicitly by the service; do not let EF infer a second relationship from the read-only domain collection.
            entity.Ignore(x => x.Credentials);
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
