using System.Security.Claims;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TroLyQuyChe.Web.Data;
using TroLyQuyChe.Web.Services;

static class SimplePolicyChecks
{
    public static async Task RunAsync(string cs)
    {
        RegulationDbContext Open() => new(new DbContextOptionsBuilder<RegulationDbContext>().UseSqlServer(cs).Options);
        var clock = new FixedClock(); var owner = "SIMPLE-" + Guid.NewGuid().ToString("N");
        var results = new List<object>(); var failures = 0;
        void Check(string name, bool ok, object? detail = null)
        {
            Console.WriteLine($"{(ok ? "PASS" : "FAIL")} {name}");
            results.Add(new { Name = name, Passed = ok, Detail = detail }); if (!ok) failures++;
        }
        async Task<QaEntry> Ask(string q, DateOnly? day = null)
        {
            await using var db = Open();
            var id = await new QuestionAnswerService(new(db), new(clock), new(), new(new(db)), clock).AskAsync(q, day, owner);
            return (await new RegulationRepository(db).FindEntryAsync(id, owner))!;
        }
        var cases = JsonSerializer.Deserialize<List<SimpleCase>>(await File.ReadAllTextAsync("docs/kiem-thu/simple-cases.json"), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        if (cases.Count < 50) throw new Exception("Bộ câu hỏi phải có ít nhất 50 câu.");
        foreach (var c in cases)
        {
            var q = await Ask(c.Question, c.Date is null ? null : DateOnly.Parse(c.Date));
            var topics = q.Citations.Select(x => x.Clause.Topic).Order().ToArray();
            var valid = q.Status.ToString() == c.Status && topics.SequenceEqual(c.Topics.Order())
                && c.Contains.All(q.Response.Contains) && q.Citations.All(x => q.Response.Contains(x.Clause.Body));
            foreach (var citation in q.Citations)
            {
                await using var db = Open();
                var source = await new CitationSourceService(new(db)).FindAsync(citation.Id, owner);
                valid &= source?.ClauseId == citation.ClauseId;
            }
            Check(c.Id + " " + c.Question, valid, new { q.Id, q.Status, q.Response, Topics = topics });
        }
        // Only the isolated demo database is deliberately changed below.
        await using var setup = Open();
        if (!setup.Database.GetDbConnection().Database.StartsWith("TroLyQuyCheDemo_Simple_", StringComparison.Ordinal))
            throw new Exception("Các bài thay nguồn chỉ chạy trên database demo riêng.");
        Check("DB01 Kho chỉ có ba văn bản mới", (await setup.Documents.OrderBy(d => d.Code).Select(d => d.Code).ToListAsync()).SequenceEqual(new[] { PolicyCatalog.LeaveCode, PolicyCatalog.OfficeCode, PolicyCatalog.TimeCode }.Order()));
        Check("DB02 Có đúng 20 điều khoản mới", await setup.Clauses.CountAsync() == 20 && !await setup.Documents.AnyAsync(d => new[] { "QC-LV", "QC-LV-BS", "QC-NP", "QC-NP-HM", "QC-TB" }.Contains(d.Code)));
        var actorId = await setup.AppUsers.Where(u => u.Username == "admin").Select(u => u.Id).SingleAsync();
        var actor = new ClaimsPrincipal(new ClaimsIdentity([new(ClaimTypes.Role, "Admin"), new("AppUserId", actorId.ToString())], "test"));
        async Task<DocumentVersion> Version(int id) { await using var db = Open(); return (await new AdminQueries(db).VersionAsync(id))!; }
        async Task<bool> Reject(Func<Task> action)
        {
            try { await action(); return false; }
            catch (Exception ex) when (ex is BusinessException or UnauthorizedAccessException or DbUpdateException or Microsoft.Data.SqlClient.SqlException) { return true; }
        }
        await using (var db = Open()) Check("AD01 Chặn quản trị không có quyền", await Reject(async () => await new VersionAdministrationService(db, clock).CreateDocumentAsync(new ClaimsPrincipal(), "FORBIDDEN", "Không được tạo")));
        var old = await Ask("Giờ bắt đầu vào làm?"); var oldCitation = old.Citations.Single();
        int draft;
        await using (var db = Open()) draft = await new VersionAdministrationService(db, clock).CreateDraftAsync(actor, 101, "2.0-demo", new(2026, 11, 11), null, 101);
        var v = await Version(draft); var stale = v.RowVersion;
        var inputs = v.Clauses.Select(c => new ClauseInput(c.ArticleNumber, c.Topic, c.Topic == "WorkStart" ? "Bắt đầu làm việc lúc 9 giờ sáng." : c.Body, c.SourceLocation)).ToList();
        await using (var db = Open()) await new VersionAdministrationService(db, clock).SaveDraftAsync(actor, draft, stale, v.VersionLabel, v.EffectiveFrom, null, inputs);
        Check("AD02 Nháp không được dùng cho câu hỏi", (await Ask("Giờ bắt đầu vào làm?")).Response.Contains("8 giờ sáng"));
        await using (var db = Open()) Check("AD03 RowVersion cũ bị chặn", await Reject(async () => await new VersionAdministrationService(db, clock).SaveDraftAsync(actor, draft, stale, v.VersionLabel, v.EffectiveFrom, null, inputs)));
        await using (var db = Open()) Check("AD04 Chặn chồng lấn nếu không chọn thay thế", await Reject(async () => await new VersionAdministrationService(db, clock).ApproveAsync(actor, draft, (await Version(draft)).RowVersion)));
        await using (var db = Open()) await new VersionAdministrationService(db, clock).ApproveAsync(actor, draft, (await Version(draft)).RowVersion, true, true);
        Check("AD05 Duyệt thay thế đổi đáp án theo SQL", (await Ask("Giờ bắt đầu vào làm?")).Response.Contains("9 giờ sáng") && (await Version(draft)).ReplacesVersionId == 101);
        Check("AD06 Thời điểm cũ vẫn dùng bản trước", (await Ask("Tháng 5/2026 giờ bắt đầu vào làm?")).Response.Contains("8 giờ sáng"));
        Check("AD07 Tháng chứa chuyển bản phải hỏi lại", (await Ask("Tháng 11/2026 giờ bắt đầu vào làm?")).Status == AnswerStatus.NeedsClarification);
        Check("AD08 Một năm sau vẫn dùng bản mới", (await Ask("Giờ bắt đầu vào làm?", new(2027, 11, 11))).Response.Contains("9 giờ sáng"));
        await using (var db = Open()) Check("AD09 Trigger đã bật lại và bảo vệ nguồn", await Reject(async () => await db.Database.ExecuteSqlRawAsync("UPDATE dbo.Clauses SET Body=N'Không được sửa' WHERE Id=1001")));
        await using (var db = Open()) await new VersionAdministrationService(db, clock).RevokeAsync(actor, draft, (await Version(draft)).RowVersion, "Kiểm tra thu hồi trên demo riêng");
        Check("AD10 Thu hồi không hồi sinh bản trước", (await Ask("Giờ bắt đầu vào làm?")).Status == AnswerStatus.InsufficientEvidence);
        await using (var db = Open()) Check("AD11 Nguồn lịch sử vẫn đúng bản cũ", (await new CitationSourceService(new(db)).FindAsync(oldCitation.Id, owner))?.Clause.Body == oldCitation.Clause.Body);
        await using (var db = Open()) Check("AD12 Đọc lại từ kết nối mới giữ dữ liệu", await db.DocumentVersions.AnyAsync(x => x.Id == draft && x.Status == VersionStatus.Revoked) && await db.VersionActions.AnyAsync(a => a.DocumentVersionId == draft && a.Action == "Approved"));
        await File.WriteAllTextAsync("docs/kiem-thu/simple-results.json", JsonSerializer.Serialize(new { Database = setup.Database.GetDbConnection().Database, QuestionCount = cases.Count, Failures = failures, Results = results }, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine($"FINAL: {cases.Count} câu; failures={failures}");
        if (failures > 0) throw new Exception("Có kiểm tra chưa đạt.");
    }
    private sealed class FixedClock : TimeProvider { public override DateTimeOffset GetUtcNow() => new(2026, 11, 11, 3, 0, 0, TimeSpan.Zero); }
    private record SimpleCase(string Id, string Question, string Status, string[] Topics, string[] Contains, string? Date);
}
