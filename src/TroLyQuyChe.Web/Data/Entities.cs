namespace TroLyQuyChe.Web.Data;

public enum VersionStatus { Draft, Approved, Revoked }
public enum AnswerStatus { Answered, NeedsClarification, InsufficientEvidence }

public class Document
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public string Title { get; set; } = "";
    public List<DocumentVersion> Versions { get; set; } = [];
}
public class DocumentVersion
{
    public int Id { get; set; }
    public int DocumentId { get; set; }
    public Document Document { get; set; } = null!;
    public string VersionLabel { get; set; } = "";
    public VersionStatus Status { get; set; }
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public int? ReplacesVersionId { get; set; }
    public DocumentVersion? ReplacesVersion { get; set; }
    public List<DocumentVersion> Replacements { get; set; } = [];
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public bool ReplacedOnStartDate => Replacements.Any(v => v.Status != VersionStatus.Draft && v.EffectiveFrom == EffectiveFrom);
    // A successor closes the lookup window without rewriting the original source.
    // Revocation of the successor does not reactivate a superseded policy.
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public DateOnly? ApplicableUntil
    {
        get
        {
            var successor = Replacements.Where(v => v.Status != VersionStatus.Draft)
                .Select(v => (DateOnly?)v.EffectiveFrom.AddDays(-1)).Min();
            return successor is null ? EffectiveTo : EffectiveTo is null ? successor
                : (EffectiveTo < successor ? EffectiveTo : successor);
        }
    }
    public List<Clause> Clauses { get; set; } = [];
    public byte[] RowVersion { get; set; } = [];
}
public class Clause
{
    public int Id { get; set; }
    public int DocumentVersionId { get; set; }
    public DocumentVersion DocumentVersion { get; set; } = null!;
    public string ArticleNumber { get; set; } = "";
    public string Body { get; set; } = "";
    public string SourceLocation { get; set; } = "";
    public string Topic { get; set; } = "";
    // Điều kiện số ngày nghỉ của quy tắc, không phải hạn mức được nghỉ.
    public decimal? MinimumLeaveDaysExclusive { get; set; }
    public decimal? MaximumLeaveDaysInclusive { get; set; }
}
public class QaEntry
{
    public long Id { get; set; }
    public string OwnerId { get; set; } = "";
    public string Question { get; set; } = "";
    public DateOnly? ApplicableFrom { get; set; }
    public DateOnly? ApplicableTo { get; set; }
    public DateTimeOffset AskedAt { get; set; }
    public string Response { get; set; } = "";
    public AnswerStatus Status { get; set; }
    public string? ProcessingTraceJson { get; set; }
    public List<Citation> Citations { get; set; } = [];
}
public class Citation
{
    public long Id { get; set; }
    public long QaEntryId { get; set; }
    public QaEntry QaEntry { get; set; } = null!;
    public int ClauseId { get; set; }
    public Clause Clause { get; set; } = null!;
}
public class AppUser
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string Role { get; set; } = "Admin";
    public bool IsActive { get; set; } = true;
}
public class VersionAction
{
    public long Id { get; set; }
    public int DocumentVersionId { get; set; }
    public DocumentVersion DocumentVersion { get; set; } = null!;
    public int ActorUserId { get; set; }
    public AppUser ActorUser { get; set; } = null!;
    public string Action { get; set; } = "";
    public string Reason { get; set; } = "";
    public DateTimeOffset OccurredAt { get; set; }
}
