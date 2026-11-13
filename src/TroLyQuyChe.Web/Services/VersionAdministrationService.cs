using System.Data;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using TroLyQuyChe.Web.Data;

namespace TroLyQuyChe.Web.Services;

public record ClauseInput(string ArticleNumber, string Topic, string Body, string SourceLocation = "", decimal? Minimum = null, decimal? Maximum = null);
public class BusinessException(string message) : Exception(message);

public class VersionAdministrationService(RegulationDbContext db, TimeProvider clock)
{
    private async Task<int> RequireAdmin(ClaimsPrincipal actor)
    {
        if (!actor.IsInRole("Admin") || !int.TryParse(actor.FindFirstValue("AppUserId"), out var id)
            || !await db.AppUsers.AsNoTracking().AnyAsync(u => u.Id == id && u.IsActive && u.Role == "Admin"))
            throw new UnauthorizedAccessException("Bạn không có quyền quản trị.");
        return id;
    }
    public async Task<int> CreateDocumentAsync(ClaimsPrincipal actor, string? code, string? title)
    {
        await RequireAdmin(actor);
        code = (code ?? "").Trim().ToUpperInvariant(); title = (title ?? "").Trim();
        if (!System.Text.RegularExpressions.Regex.IsMatch(code, "^[A-Z0-9-]{2,30}$") || title.Length is < 1 or > 300)
            throw new BusinessException("Mã gồm 2–30 chữ in hoa/số/dấu gạch; tên tối đa 300 ký tự.");
        if (await db.Documents.AnyAsync(d => d.Code == code)) throw new BusinessException("Mã văn bản đã tồn tại.");
        var doc = new Document { Code = code, Title = title }; db.Documents.Add(doc);
        await db.SaveChangesAsync(); return doc.Id;
    }
    public async Task<int> CreateDraftAsync(ClaimsPrincipal actor, int documentId, string? label, DateOnly from, DateOnly? to, int? copyFrom = null)
    {
        var actorId = await RequireAdmin(actor);
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        await LockDocument(documentId);
        label = label?.Trim() ?? "";
        ValidateDates(label, from, to);
        if (await db.DocumentVersions.AnyAsync(v => v.DocumentId == documentId && v.VersionLabel == label.Trim())) throw new BusinessException("Nhãn phiên bản đã tồn tại.");
        var v = new DocumentVersion { DocumentId = documentId, VersionLabel = label.Trim(), EffectiveFrom = from, EffectiveTo = to };
        if (copyFrom.HasValue)
        {
            var source = await db.DocumentVersions.AsNoTracking().Include(x => x.Clauses).SingleOrDefaultAsync(x => x.Id == copyFrom && x.DocumentId == documentId)
                ?? throw new BusinessException("Không tìm thấy phiên bản gốc trong văn bản.");
            v.Clauses = source.Clauses.Select(c => new Clause { ArticleNumber = c.ArticleNumber, Topic = c.Topic, Body = c.Body, SourceLocation = c.SourceLocation,
                MinimumLeaveDaysExclusive = c.MinimumLeaveDaysExclusive, MaximumLeaveDaysInclusive = c.MaximumLeaveDaysInclusive }).ToList();
        }
        db.DocumentVersions.Add(v); await db.SaveChangesAsync();
        Audit(v.Id, actorId, "Created", "Tạo bản nháp"); await db.SaveChangesAsync(); await tx.CommitAsync(); return v.Id;
    }
    public async Task SaveDraftAsync(ClaimsPrincipal actor, int id, byte[] expected, string label, DateOnly from, DateOnly? to, IReadOnlyList<ClauseInput> clauses)
    {
        var actorId = await RequireAdmin(actor);
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var v = await LockedVersion(id);
        CheckDraft(v, expected); ValidateDates(label, from, to);
        if (await db.DocumentVersions.AnyAsync(x => x.DocumentId == v.DocumentId && x.Id != id && x.VersionLabel == label.Trim())) throw new BusinessException("Nhãn phiên bản đã tồn tại.");
        if (clauses.GroupBy(c => c.ArticleNumber.Trim(), StringComparer.OrdinalIgnoreCase).Any(g => g.Count() > 1)) throw new BusinessException("Số điều khoản không được trùng nhau.");
        foreach (var c in clauses)
            if (string.IsNullOrWhiteSpace(c.ArticleNumber) || c.ArticleNumber.Length > 50 || !PolicyCatalog.TopicsFor(v.Document.Code).Contains(c.Topic)
                || string.IsNullOrWhiteSpace(c.Body) || c.Body.Length > 10000 || c.SourceLocation.Length > 400
                || (c.Topic == "LeaveNotice" && ((c.Minimum.HasValue && c.Minimum.Value != decimal.Truncate(c.Minimum.Value)) || (c.Maximum.HasValue && c.Maximum.Value != decimal.Truncate(c.Maximum.Value))))
                || c.Minimum < 0 || c.Maximum <= 0 || c.Minimum > Understanding.LeaveDurationParser.MaximumDays || c.Maximum > Understanding.LeaveDurationParser.MaximumDays
                || (c.Topic != "LeaveNotice" && (c.Minimum.HasValue || c.Maximum.HasValue))
                || (c.Minimum.HasValue && c.Maximum.HasValue && c.Maximum <= c.Minimum))
                throw new BusinessException("Điều khoản cần số điều, chủ đề hợp lệ, nội dung tối đa 10000 ký tự và khoảng ngày nghỉ hợp lệ.");
        // Xóa/thêm chỉ trên Draft, trước khi nguồn có thể được trích dẫn.
        db.Clauses.RemoveRange(v.Clauses); await db.SaveChangesAsync();
        v.Clauses = clauses.Select(c => new Clause { ArticleNumber = c.ArticleNumber.Trim(), Topic = c.Topic, Body = c.Body.Trim(), SourceLocation = c.SourceLocation.Trim(),
            MinimumLeaveDaysExclusive = c.Minimum, MaximumLeaveDaysInclusive = c.Maximum }).ToList();
        v.VersionLabel = label.Trim(); v.EffectiveFrom = from; v.EffectiveTo = to;
        db.Entry(v).Property(x => x.VersionLabel).IsModified = true;
        Audit(id, actorId, "Edited", "Lưu bản nháp"); await db.SaveChangesAsync(); await tx.CommitAsync();
    }
    public async Task ApproveAsync(ClaimsPrincipal actor, int id, byte[] expected,
        bool replaceCurrent = false, bool effectiveOnApproval = false, bool acknowledgeMissingTopics = false)
    {
        var actorId = await RequireAdmin(actor);
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var v = await LockedVersion(id); CheckDraft(v, expected);
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeBySystemTimeZoneId(clock.GetUtcNow(), "SE Asia Standard Time").DateTime);
        if (effectiveOnApproval) { v.EffectiveFrom = today; v.EffectiveTo = null; }
        ValidateDates(v.VersionLabel, v.EffectiveFrom, v.EffectiveTo);
        if (v.Clauses.Count == 0) throw new BusinessException("Bản nháp phải có ít nhất một điều khoản.");
        if (v.Clauses.Where(c => c.Topic != "LeaveNotice").GroupBy(c => c.Topic).Any(g => g.Count() > 1))
            throw new BusinessException("Mỗi chủ đề chỉ có một điều khoản tổng hợp; riêng báo trước được chia theo các khoảng ngày không chồng nhau.");
        if (ClauseValidation.NoticeProblem(v.Clauses) is { } problem) throw new BusinessException(problem);
        var known = await db.DocumentVersions.AsNoTracking().Where(x => x.DocumentId == v.DocumentId && x.Id != id)
            .Include(x => x.Clauses).ToListAsync();
        v.Document = await db.Documents.SingleAsync(d => d.Id == v.DocumentId);
        var missing = ClauseValidation.MissingTopics(v, known);
        if (missing.Length > 0 && !acknowledgeMissingTopics)
            throw new BusinessException($"Bản này thiếu các chủ đề: {string.Join(", ", missing)}. Sao chép/bổ sung nội dung, hoặc xác nhận rõ việc bỏ chủ đề trước khi duyệt.");
        var others = await db.DocumentVersions.Where(x => x.DocumentId == v.DocumentId && x.Id != id && x.Status == VersionStatus.Approved)
            .Include(x => x.Replacements).ToListAsync();
        var overlaps = others.Where(x => (v.EffectiveTo == null || x.EffectiveFrom <= v.EffectiveTo)
            && (x.ApplicableUntil == null || x.ApplicableUntil >= v.EffectiveFrom)).ToList();
        if (overlaps.Count > 0)
        {
            if (!replaceCurrent) throw new BusinessException("Không thể duyệt: khoảng hiệu lực chồng lấn phiên bản đã duyệt. Chọn thay thế nếu muốn ban hành bản kế tiếp.");
            if (overlaps.Count != 1 || overlaps[0].EffectiveFrom > v.EffectiveFrom
                || overlaps[0].Replacements.Count > 0 || v.EffectiveFrom < today)
                throw new BusinessException("Chỉ thay thế một phiên bản từ hôm nay hoặc một ngày tương lai, không sớm hơn ngày bắt đầu của bản trước và chưa có bản kế tiếp. Hãy kiểm tra lịch hiệu lực.");
            v.ReplacesVersionId = overlaps[0].Id;
            Audit(overlaps[0].Id, actorId, "Superseded", $"Được thay thế từ {v.EffectiveFrom:dd/MM/yyyy} bởi phiên bản {v.VersionLabel}" + (overlaps[0].EffectiveFrom == v.EffectiveFrom ? "; đính chính trong cùng ngày, giữ nguyên nguồn lịch sử" : ""));
        }
        v.ApprovedAt = clock.GetUtcNow(); v.Status = VersionStatus.Approved;
        Audit(id, actorId, "Approved", v.ReplacesVersionId.HasValue ? "Duyệt bản thay thế; giữ nguyên nguồn cũ" : "Duyệt phiên bản");
        await db.SaveChangesAsync(); await tx.CommitAsync();
    }
    public async Task RevokeAsync(ClaimsPrincipal actor, int id, byte[] expected, string reason)
    {
        var actorId = await RequireAdmin(actor);
        if (string.IsNullOrWhiteSpace(reason) || reason.Length > 1000) throw new BusinessException("Nhập lý do thu hồi (tối đa 1000 ký tự).");
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var v = await LockedVersion(id);
        CheckRevision(v, expected);
        if (v.Status != VersionStatus.Approved) throw new BusinessException("Chỉ thu hồi phiên bản đã duyệt.");
        v.Status = VersionStatus.Revoked; Audit(id, actorId, "Revoked", reason.Trim()); await db.SaveChangesAsync(); await tx.CommitAsync();
    }
    private async Task<DocumentVersion> LockedVersion(int id)
    {
        // Chỉ đọc khóa định tuyến ở đây; trạng thái và nội dung sẽ đọc lại sau khóa văn bản.
        var documentId = await db.DocumentVersions.FromSqlInterpolated($"SELECT * FROM dbo.DocumentVersions WITH (NOLOCK) WHERE Id={id}").AsNoTracking().Select(v => (int?)v.DocumentId).SingleOrDefaultAsync()
            ?? throw new BusinessException("Không tìm thấy phiên bản.");
        await LockDocument(documentId);
        // Service uses a scoped context. Clear stale tracked state before reloading under lock.
        db.ChangeTracker.Clear();
        return await db.DocumentVersions.Include(v => v.Document).Include(v => v.Clauses).SingleAsync(v => v.Id == id);
    }
    private async Task LockDocument(int id)
    {
        if (await db.Documents.FromSqlInterpolated($"SELECT * FROM dbo.Documents WITH (UPDLOCK,HOLDLOCK) WHERE Id={id}").SingleOrDefaultAsync() is null)
            throw new BusinessException("Không tìm thấy văn bản.");
    }
    private static void ValidateDates(string label, DateOnly from, DateOnly? to)
    {
        if (string.IsNullOrWhiteSpace(label) || label.Trim().Length > 30 || from == default || to < from) throw new BusinessException("Nhãn phiên bản hoặc khoảng hiệu lực không hợp lệ.");
    }
    private static void CheckRevision(DocumentVersion v, byte[] expected)
    {
        if (!v.RowVersion.SequenceEqual(expected)) throw new BusinessException("Dữ liệu đã thay đổi. Hãy tải lại trang trước khi thao tác.");
    }
    private static void CheckDraft(DocumentVersion v, byte[] expected)
    {
        CheckRevision(v, expected);
        if (v.Status != VersionStatus.Draft) throw new BusinessException("Chỉ được sửa/duyệt bản nháp. Nội dung đã duyệt được giữ nguyên.");
    }
    private void Audit(int id, int actor, string action, string reason) => db.VersionActions.Add(new VersionAction { DocumentVersionId = id, ActorUserId = actor, Action = action, Reason = reason, OccurredAt = clock.GetUtcNow() });
}
