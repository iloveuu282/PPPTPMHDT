using Microsoft.EntityFrameworkCore;

namespace TroLyQuyChe.Web.Data;

public class RegulationDbContext(DbContextOptions<RegulationDbContext> options) : DbContext(options)
{
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<DocumentVersion> DocumentVersions => Set<DocumentVersion>();
    public DbSet<Clause> Clauses => Set<Clause>();
    public DbSet<QaEntry> QaEntries => Set<QaEntry>();
    public DbSet<Citation> Citations => Set<Citation>();
    public DbSet<AppUser> AppUsers => Set<AppUser>();
    public DbSet<VersionAction> VersionActions => Set<VersionAction>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Document>().Property(x => x.Code).HasMaxLength(30);
        b.Entity<Document>().Property(x => x.Title).HasMaxLength(300);
        b.Entity<Document>().HasIndex(x => x.Code).IsUnique();
        b.Entity<DocumentVersion>().Property(x => x.VersionLabel).HasMaxLength(30);
        b.Entity<DocumentVersion>().Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
        b.Entity<DocumentVersion>().Property(x => x.RowVersion).IsRowVersion();
        b.Entity<DocumentVersion>().HasOne(v => v.ReplacesVersion).WithMany(v => v.Replacements)
            .HasForeignKey(v => v.ReplacesVersionId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<DocumentVersion>().HasIndex(v => v.ReplacesVersionId).IsUnique().HasFilter("[ReplacesVersionId] IS NOT NULL");
        b.Entity<DocumentVersion>().ToTable(t => t.UseSqlOutputClause(false));
        b.Entity<Clause>().ToTable(t => t.UseSqlOutputClause(false));
        b.Entity<Document>().ToTable(t => t.UseSqlOutputClause(false));
        b.Entity<DocumentVersion>().HasIndex(x => new { x.DocumentId, x.VersionLabel }).IsUnique();
        b.Entity<DocumentVersion>().ToTable(t => t.HasCheckConstraint("CK_Version_Dates", "[EffectiveTo] IS NULL OR [EffectiveTo] >= [EffectiveFrom]"));
        b.Entity<Clause>().Property(x => x.ArticleNumber).HasMaxLength(50);
        b.Entity<Clause>().Property(x => x.SourceLocation).HasMaxLength(400);
        b.Entity<Clause>().Property(x => x.Topic).HasMaxLength(80);
        b.Entity<Clause>().Property(x => x.MinimumLeaveDaysExclusive).HasPrecision(8, 2);
        b.Entity<Clause>().Property(x => x.MaximumLeaveDaysInclusive).HasPrecision(8, 2);
        b.Entity<Clause>().HasIndex(x => new { x.DocumentVersionId, x.ArticleNumber }).IsUnique();
        b.Entity<QaEntry>().Property(x => x.OwnerId).HasMaxLength(64);
        b.Entity<QaEntry>().Property(x => x.Question).HasMaxLength(2000);
        b.Entity<QaEntry>().Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
        b.Entity<QaEntry>().HasIndex(x => new { x.OwnerId, x.AskedAt });
        b.Entity<Citation>().HasIndex(x => new { x.QaEntryId, x.ClauseId }).IsUnique();
        b.Entity<Citation>().HasOne(x => x.Clause).WithMany().HasForeignKey(x => x.ClauseId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Citation>().HasOne(x => x.QaEntry).WithMany(x => x.Citations).HasForeignKey(x => x.QaEntryId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Clause>().HasOne(x => x.DocumentVersion).WithMany(x => x.Clauses).HasForeignKey(x => x.DocumentVersionId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<DocumentVersion>().HasOne(x => x.Document).WithMany(x => x.Versions).HasForeignKey(x => x.DocumentId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<AppUser>().HasIndex(x => x.Username).IsUnique();
        b.Entity<AppUser>().Property(x => x.Username).HasMaxLength(80);
        b.Entity<AppUser>().Property(x => x.Role).HasMaxLength(30);
        b.Entity<VersionAction>().Property(x => x.Action).HasMaxLength(30);
        b.Entity<VersionAction>().Property(x => x.Reason).HasMaxLength(1000);
        b.Entity<VersionAction>().HasOne(x => x.DocumentVersion).WithMany().HasForeignKey(x => x.DocumentVersionId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<VersionAction>().HasOne(x => x.ActorUser).WithMany().HasForeignKey(x => x.ActorUserId).OnDelete(DeleteBehavior.Restrict);
        SimplifiedSeedData.Configure(b);
    }
}
