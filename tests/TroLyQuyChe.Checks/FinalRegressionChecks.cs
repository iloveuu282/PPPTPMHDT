using System.Security.Claims;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TroLyQuyChe.Web.Data;
using TroLyQuyChe.Web.Services;
using TroLyQuyChe.Web.Services.Understanding;

static class FinalRegressionChecks
{
    private sealed class Clock : TimeProvider { public override DateTimeOffset GetUtcNow() => new(2026, 11, 11, 3, 0, 0, TimeSpan.Zero); }
    public static async Task QueriesAsync(string cs)
    {
        var cases = JsonSerializer.Deserialize<List<Case>>(await File.ReadAllTextAsync("docs/kiem-thu/final-regression-cases.json"))!;
        var results = new List<object>(); int failed = 0;
        foreach (var c in cases)
        {
            await using var db = Open(cs); var clock = new Clock();
            var owner = Guid.NewGuid().ToString("N");
            var id = await new QuestionAnswerService(new(db), new(clock), new(), new(new(db)), clock)
                .AskAsync(c.Question, c.Date is null ? null : DateOnly.Parse(c.Date), owner);
            var entry = (await new RegulationRepository(db).FindEntryAsync(id, owner))!;
            var topics = entry.Citations.Select(x => x.Clause.Topic).Order().ToArray();
            var ok = entry.Status.ToString() == c.Status && topics.SequenceEqual(c.Topics.Order());
            if (!ok) failed++;
            Console.WriteLine($"{(ok ? "PASS" : "FAIL")} REG {c.Question}");
            results.Add(new { c.Question, Passed = ok, entry.Status, entry.Response, Topics = topics });
        }
        await File.WriteAllTextAsync("docs/kiem-thu/final-regression-results.json", JsonSerializer.Serialize(new { Count = cases.Count, Failed = failed, Results = results }, new JsonSerializerOptions { WriteIndented = true }));
        if (failed > 0) throw new Exception($"{failed} câu hồi quy chưa đạt.");
    }
    private static RegulationDbContext Open(string cs) => new(new DbContextOptionsBuilder<RegulationDbContext>().UseSqlServer(cs).Options);
    private record Case(string Question, string Status, string[] Topics, string? Date);

    public static async Task AdminAsync(string cs)
    {
        await using var setup = Open(cs);
        if (!setup.Database.GetDbConnection().Database.StartsWith("TroLyQuyCheDemo_Simple_")) throw new Exception("Chỉ chạy trên database thử nghiệm.");
        var actorId = await setup.AppUsers.Where(x => x.Username == "admin").Select(x => x.Id).SingleAsync();
        var actor = new ClaimsPrincipal(new ClaimsIdentity([new(ClaimTypes.Role, "Admin"), new("AppUserId", actorId.ToString())], "test"));
        var clock = new Clock(); var checks = new List<object>(); var failed = 0;
        void Check(string name, bool ok) { checks.Add(new { Name = name, Passed = ok }); Console.WriteLine($"{(ok ? "PASS" : "FAIL")} {name}"); if (!ok) failed++; }
        async Task<DocumentVersion> Load(int id) { await using var db = Open(cs); return (await new AdminQueries(db).VersionAsync(id))!; }
        async Task<bool> Rejected(Func<Task> action) { try { await action(); return false; } catch (BusinessException) { return true; } }
        async Task<int> Draft(int doc, string label, int? copy = null)
        { await using var db = Open(cs); return await new VersionAdministrationService(db, clock).CreateDraftAsync(actor, doc, label, new(2026, 11, 11), null, copy); }
        async Task Save(int id, ClauseInput[] inputs)
        { var v = await Load(id); await using var db = Open(cs); await new VersionAdministrationService(db, clock).SaveDraftAsync(actor, id, v.RowVersion, v.VersionLabel, v.EffectiveFrom, null, inputs); }
        async Task Approve(int id, bool replace = false, bool ack = false)
        { var v = await Load(id); await using var db = Open(cs); await new VersionAdministrationService(db, clock).ApproveAsync(actor, id, v.RowVersion, replace, false, ack); }
        var id = await Draft(102, "bad-ranges", 102); var v = await Load(id);
        ClauseInput[] Inputs(Func<Clause, ClauseInput> transform) => v.Clauses.Select(transform).ToArray();
        ClauseInput Copy(Clause c) => new(c.ArticleNumber, c.Topic, c.Body, c.SourceLocation, c.MinimumLeaveDaysExclusive, c.MaximumLeaveDaysInclusive);
        await Save(id, Inputs(c => c.Topic == "LeaveNotice" ? Copy(c) with { Minimum = null, Maximum = null } : Copy(c)));
        Check("FAD01 Chặn duyệt khoảng báo trước chồng nhau", await Rejected(() => Approve(id, true)));
        await Save(id, Inputs(c => c.Topic == "LeaveNotice" && c.MinimumLeaveDaysExclusive == 1 ? Copy(c) with { Minimum = 2 } : Copy(c)));
        Check("FAD02 Chặn khoảng báo trước bỏ sót ngày", await Rejected(() => Approve(id, true)));
        Check("FAD03 Chặn ngưỡng quá lớn trước khi ghi SQL", await Rejected(() => Save(id, Inputs(c => c.Topic == "LeaveNotice" ? Copy(c) with { Minimum = 1000000 } : Copy(c)))));
        Check("FAD04 Chặn ngưỡng lẻ ngày", await Rejected(() => Save(id, Inputs(c => c.Topic == "LeaveNotice" ? Copy(c) with { Minimum = 0.5m } : Copy(c)))));
        Check("FAD05 Chặn gán sai chủ đề cho văn bản", await Rejected(() => Save(id, [new("Điều 1", "WorkStart", "Nội dung sai chủ đề")])));
        Check("FAD06 Chặn duyệt nháp rỗng", await Rejected(async () => await Approve(await Draft(103, "empty"))));

        var partial = await Draft(103, "partial", 103);
        var office = await Load(partial);
        await Save(partial, office.Clauses.Where(c => c.Topic != "Visitors").Select(Copy).ToArray());
        Check("FAD07 Thiếu chủ đề phải xác nhận rõ", await Rejected(() => Approve(partial, true)));
        await Approve(partial, true, true);
        Check("FAD08 Cho phép bỏ chủ đề khi chủ động xác nhận", (await Load(partial)).Status == VersionStatus.Approved);
        long oldCitation;
        const string historyOwner = "final-same-day-owner";
        await using (var db = Open(cs))
        {
            var qid = await new QuestionAnswerService(new(db), new(clock), new(), new(new(db)), clock).AskAsync("Vệ sinh chỗ làm thế nào?", null, historyOwner);
            oldCitation = (await new RegulationRepository(db).FindEntryAsync(qid, historyOwner))!.Citations.Single().Id;
        }
        var sameDay = await Draft(103, "same-day-full", 103);
        await Approve(sameDay, true);
        var before = await Load(partial);
        Check("FAD09 Đính chính cùng ngày giữ nguồn cũ", before.ReplacedOnStartDate && before.Clauses.Count == 6);
        await using (var db = Open(cs))
        {
            var answer = await new PolicyLookupService(new(db)).LookupAsync(new QuestionRecognizer().Recognize("Khách vào công ty thế nào?"), new(2026, 11, 11), new(2026, 11, 11));
            Check("FAD10 Câu mới dùng đúng bản đính chính", answer.Status == AnswerStatus.Answered && answer.Sources.All(c => c.DocumentVersionId == sameDay));
        }
        int doc;
        await using (var db = Open(cs)) doc = await new VersionAdministrationService(db, clock).CreateDocumentAsync(actor, "CONCURRENT-FINAL", "Kiểm tra đồng thời");
        var d1 = await Draft(doc, "a"); var d2 = await Draft(doc, "b");
        foreach (var d in new[] { d1, d2 }) await Save(d, [new("Điều 1", "WorkStart", "Nội dung thử nghiệm")]);
        async Task<bool> TryApprove(int d) { try { await Approve(d); return true; } catch (BusinessException) { return false; } }
        var concurrent = await Task.WhenAll(TryApprove(d1), TryApprove(d2));
        Check("FAD11 Duyệt đồng thời chỉ một bản thành công", concurrent.Count(x => x) == 1);
        await using (var db = Open(cs))
        {
            var source = await new CitationSourceService(new(db)).FindAsync(oldCitation, historyOwner);
            Check("FAD12 Lịch sử giữ nguồn trước đính chính cùng ngày", source?.Clause.DocumentVersionId == partial && source.Clause.DocumentVersion.ReplacedOnStartDate);
        }
        await using (var db = Open(cs)) await new VersionAdministrationService(db, clock).RevokeAsync(actor, sameDay, (await Load(sameDay)).RowVersion, "Thử thu hồi đính chính cùng ngày");
        await using (var db = Open(cs))
        {
            var answer = await new PolicyLookupService(new(db)).LookupAsync(new QuestionRecognizer().Recognize("Vệ sinh chỗ làm thế nào?"), new(2026, 11, 11), new(2026, 11, 11));
            Check("FAD13 Thu hồi đính chính không hồi sinh bản trước", answer.Status == AnswerStatus.InsufficientEvidence);
        }
        await File.WriteAllTextAsync("docs/kiem-thu/final-admin-results.json", JsonSerializer.Serialize(new { Failed = failed, Checks = checks }, new JsonSerializerOptions { WriteIndented = true }));
        if (failed > 0) throw new Exception("Kiểm tra quản trị cuối chưa đạt.");
    }
}
