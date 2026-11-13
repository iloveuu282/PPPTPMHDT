-- Chỉ đọc. Đổi USE nếu đang trình diễn trên database TroLyQuyCheDemo_Final_...
USE [TroLyQuyCheDb];
GO
DECLARE @Ngay date=CONVERT(date,DATEADD(hour,7,SYSUTCDATETIME()));
SELECT DB_NAME() AS DatabaseDangXem,@Ngay AS NgayVietNam;
-- Các phiên bản thực sự đang áp dụng; không chỉ nhìn Status.
SELECT d.Code,d.Title,v.Id,v.VersionLabel,v.ApprovedAt,v.EffectiveFrom,v.EffectiveTo
FROM Documents d JOIN DocumentVersions v ON v.DocumentId=d.Id
WHERE v.Status='Approved' AND v.EffectiveFrom<=@Ngay AND (v.EffectiveTo IS NULL OR v.EffectiveTo>=@Ngay)
AND NOT EXISTS(SELECT 1 FROM DocumentVersions n WHERE n.ReplacesVersionId=v.Id AND n.Status<>'Draft' AND n.EffectiveFrom<=@Ngay);
-- Nguyên văn điều khoản và cận thời lượng nghỉ của toàn bộ nguồn đang lưu.
SELECT d.Code,v.Id AS VersionId,v.VersionLabel,v.Status,v.ReplacesVersionId,
 c.Id AS ClauseId,c.ArticleNumber,c.Topic,c.Body,c.MinimumLeaveDaysExclusive,c.MaximumLeaveDaysInclusive
FROM Documents d JOIN DocumentVersions v ON v.DocumentId=d.Id
LEFT JOIN Clauses c ON c.DocumentVersionId=v.Id ORDER BY d.Code,v.Id,c.Id;
-- Mỗi lần Hỏi hợp lệ thêm một lượt, và chỉ câu có căn cứ có Citation.
SELECT TOP(30) Id,Question,Response,Status,ApplicableFrom,ApplicableTo,AskedAt
FROM QaEntries ORDER BY Id DESC;
SELECT TOP(60) q.Id AS QuestionId,ci.Id AS CitationId,c.Id AS ClauseId,d.Code,
 v.VersionLabel,c.ArticleNumber,c.Body
FROM QaEntries q JOIN Citations ci ON ci.QaEntryId=q.Id
JOIN Clauses c ON c.Id=ci.ClauseId JOIN DocumentVersions v ON v.Id=c.DocumentVersionId
JOIN Documents d ON d.Id=v.DocumentId ORDER BY q.Id DESC,ci.Id;
-- Dấu vết quyết định; chỉ xem nội bộ để vấn đáp.
SELECT TOP(3) Id,Question,ProcessingTraceJson FROM QaEntries ORDER BY Id DESC;
SELECT TOP(30) a.Id,a.DocumentVersionId,u.Username,a.Action,a.Reason,a.OccurredAt
FROM VersionActions a JOIN AppUsers u ON a.ActorUserId=u.Id ORDER BY a.Id DESC;
SELECT Username,Role,IsActive FROM AppUsers;
SELECT name,is_disabled FROM sys.triggers WHERE parent_id IN
 (OBJECT_ID('Documents'),OBJECT_ID('DocumentVersions'),OBJECT_ID('Clauses'));
SELECT MigrationId FROM __EFMigrationsHistory ORDER BY MigrationId;
