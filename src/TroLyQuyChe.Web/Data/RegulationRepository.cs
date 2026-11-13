using Microsoft.EntityFrameworkCore;

namespace TroLyQuyChe.Web.Data;

public class RegulationRepository(RegulationDbContext db)
{
    public async Task<List<DocumentVersion>> OverlappingApprovedAsync(string code, DateOnly from, DateOnly to)
    {
        var candidates = await db.DocumentVersions.AsNoTracking()
            .Where(v => v.Document.Code == code && v.Status == VersionStatus.Approved && v.EffectiveFrom <= to && (v.EffectiveTo == null || v.EffectiveTo >= from))
            .Include(v => v.Clauses).Include(v => v.Replacements).OrderBy(v => v.EffectiveFrom).ToListAsync();
        return candidates.Where(v => !v.ReplacedOnStartDate && (v.ApplicableUntil == null || v.ApplicableUntil >= from)).ToList();
    }

    public async Task<long> SaveAnswerAsync(QaEntry entry)
    {
        await using var transaction = await db.Database.BeginTransactionAsync();
        db.QaEntries.Add(entry);
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        return entry.Id;
    }
    public Task<List<QaEntry>> RecentAsync(string owner) => db.QaEntries.AsNoTracking()
        .Where(q => q.OwnerId == owner).OrderByDescending(q => q.Id).Take(30).ToListAsync();
    public Task<QaEntry?> FindEntryAsync(long id, string owner) => db.QaEntries.AsNoTracking()
        .Include(q => q.Citations).ThenInclude(c => c.Clause).ThenInclude(c => c.DocumentVersion).ThenInclude(v => v.Document)
        .SingleOrDefaultAsync(q => q.Id == id && q.OwnerId == owner);
    // BR-06: không chọn lại phiên bản theo thời gian/trạng thái hiện tại khi mở lịch sử.
    public Task<Citation?> FindCitationAsync(long id, string owner, bool isAdmin = false) => db.Citations.AsNoTracking()
        .Include(c => c.Clause).ThenInclude(c => c.DocumentVersion).ThenInclude(v => v.Replacements)
        .Include(c => c.Clause).ThenInclude(c => c.DocumentVersion).ThenInclude(v => v.Document)
        .SingleOrDefaultAsync(c => c.Id == id && (isAdmin || c.QaEntry.OwnerId == owner));
}
