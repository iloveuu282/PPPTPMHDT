using TroLyQuyChe.Web.Data;

namespace TroLyQuyChe.Web.Services;

public class HistoryService(RegulationRepository repository)
{
    public Task<List<QaEntry>> RecentAsync(string owner) => repository.RecentAsync(owner);
    public Task<QaEntry?> FindAsync(long id, string owner) => repository.FindEntryAsync(id, owner);
}
public class CitationSourceService(RegulationRepository repository)
{
    public Task<Citation?> FindAsync(long id, string owner, bool isAdmin = false) => repository.FindCitationAsync(id, owner, isAdmin);
}
