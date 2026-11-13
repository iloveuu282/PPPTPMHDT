using System.Security.Claims;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TroLyQuyChe.Web.Data;
using TroLyQuyChe.Web.Services;

static class FinalizeDemoData
{
    public static async Task<int> CompleteTimeAsync(RegulationDbContext db)
    {
        var definitions = JsonSerializer.Deserialize<List<Definition>>(await File.ReadAllTextAsync("docs/nghiep-vu/06_BoQuyChe_ChotDemo.json"), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        var definition = definitions.Single(d => d.Code == PolicyCatalog.TimeCode);
        var document = await db.Documents.SingleAsync(d => d.Code == definition.Code);
        var existing = await db.DocumentVersions.Include(v => v.Clauses).Include(v => v.Replacements)
            .Where(v => v.DocumentId == document.Id).ToListAsync();
        var finished = existing.SingleOrDefault(v => v.VersionLabel == definition.Version);
        if (finished is not null)
        {
            if (finished.Status == VersionStatus.Approved && definition.Clauses.All(c => finished.Clauses.Any(x => x.Topic == c.Topic && x.Body == c.Body))) return finished.Id;
            throw new BusinessException($"Đã có bản {definition.Version} khác nội dung dự kiến; cần xem lại trước khi chốt, không tự ghi đè.");
        }
        var clock = TimeProvider.System;
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeBySystemTimeZoneId(clock.GetUtcNow(), "SE Asia Standard Time").DateTime);
        var current = existing.Single(v => v.Status == VersionStatus.Approved && !v.ReplacedOnStartDate && v.EffectiveFrom <= today && (v.ApplicableUntil == null || v.ApplicableUntil >= today));
        var actorId = await db.AppUsers.Where(u => u.Username == "admin" && u.IsActive).Select(u => u.Id).SingleAsync();
        var actor = new ClaimsPrincipal(new ClaimsIdentity([new(ClaimTypes.Role, "Admin"), new("AppUserId", actorId.ToString())], "local-finalize"));
        var service = new VersionAdministrationService(db, clock);
        var id = await service.CreateDraftAsync(actor, document.Id, definition.Version, today, null, current.Id);
        db.ChangeTracker.Clear();
        var draft = await db.DocumentVersions.AsNoTracking().SingleAsync(v => v.Id == id);
        await service.SaveDraftAsync(actor, id, draft.RowVersion, draft.VersionLabel, today, null,
            definition.Clauses.Select(c => new ClauseInput(c.Article, c.Topic, c.Body, $"Bộ quy chế nội bộ Mai Việt Hoàng • Thời gian làm việc • {definition.Version}")).ToArray());
        db.ChangeTracker.Clear(); draft = await db.DocumentVersions.AsNoTracking().SingleAsync(v => v.Id == id);
        await service.ApproveAsync(actor, id, draft.RowVersion, true, true);
        return id;
    }
    private record Definition(string Code, string Version, List<ClauseDefinition> Clauses);
    private record ClauseDefinition(string Article, string Topic, string Body);
}
