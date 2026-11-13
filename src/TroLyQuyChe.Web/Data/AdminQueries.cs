using Microsoft.EntityFrameworkCore;

namespace TroLyQuyChe.Web.Data;

public class AdminQueries(RegulationDbContext db)
{
    public Task<List<DocumentVersion>> VersionsWithClausesAsync(int documentId) => db.DocumentVersions.AsNoTracking()
        .Where(v => v.DocumentId == documentId).Include(v => v.Clauses).ToListAsync();
    public Task<List<Document>> DocumentsAsync() => db.Documents.AsNoTracking().Include(d => d.Versions).ThenInclude(v => v.Replacements).OrderBy(d => d.Code).ToListAsync();
    public Task<DocumentVersion?> VersionAsync(int id) => db.DocumentVersions.AsNoTracking().Include(v => v.Document).Include(v => v.Replacements).Include(v => v.Clauses).SingleOrDefaultAsync(v => v.Id == id);
    public Task<List<VersionAction>> ActionsAsync(int? version = null) => db.VersionActions.AsNoTracking().Include(a => a.ActorUser).Include(a => a.DocumentVersion).ThenInclude(v => v.Document)
        .Where(a => version == null || a.DocumentVersionId == version).OrderByDescending(a => a.Id).Take(100).ToListAsync();
    public Task<QaEntry?> EntryAsync(long id) => db.QaEntries.AsNoTracking()
        .Include(q => q.Citations).ThenInclude(c => c.Clause).ThenInclude(c => c.DocumentVersion).ThenInclude(v => v.Document)
        .SingleOrDefaultAsync(q => q.Id == id);
    public Task<AppUser?> UserAsync(string name) => db.AppUsers.AsNoTracking().SingleOrDefaultAsync(u => u.Username == name && u.IsActive);
    public Task<List<QaEntry>> HistoryAsync() => db.QaEntries.AsNoTracking().Include(q => q.Citations).ThenInclude(c => c.Clause).ThenInclude(c => c.DocumentVersion).ThenInclude(v => v.Document)
        .OrderByDescending(q => q.Id).Take(100).ToListAsync();
}
