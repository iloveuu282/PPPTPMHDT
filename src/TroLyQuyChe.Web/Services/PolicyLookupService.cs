using TroLyQuyChe.Web.Data;
using TroLyQuyChe.Web.Services.Understanding;

namespace TroLyQuyChe.Web.Services;

public record LookupStep(string DocumentCode, string Description, int[] VersionIds, int[] ClauseIds);
public record PolicyAnswer(AnswerStatus Status, string Response, IReadOnlyList<Clause> Sources,
    IReadOnlyList<LookupStep>? Steps = null);

public class PolicyLookupService(RegulationRepository repository)
{
    public async Task<PolicyAnswer> LookupAsync(RecognizedQuestion question, DateOnly from, DateOnly to)
    {
        var steps = new List<LookupStep>();
        PolicyAnswer Missing(string content) => new(AnswerStatus.InsufficientEvidence,
            $"Chưa đủ căn cứ để trả lời: kho quy chế chưa có điều khoản phù hợp quy định {content}. Không suy ra nội dung này từ điều khoản khác.", [], steps);
        if (question.Clarification is not null) return new(AnswerStatus.NeedsClarification, question.Clarification, []);
        if (question.MissingEvidence is not null) return Missing(question.MissingEvidence);
        var bindings = question.Intents.Select(i => PolicyCatalog.Bindings[i]).ToList();
        var sources = new List<Clause>();
        foreach (var group in bindings.GroupBy(b => b.DocumentCode))
        {
            var versions = await repository.OverlappingApprovedAsync(group.Key, from, to);
            var covering = versions.Where(v => v.EffectiveFrom <= from && (v.ApplicableUntil == null || v.ApplicableUntil >= to)).ToList();
            steps.Add(new(group.Key, $"Sau lọc trạng thái đã duyệt và khoảng tra cứu: {versions.Count} bản giao khoảng; {covering.Count} bản bao phủ toàn khoảng.", versions.Select(v => v.Id).ToArray(), []));
            if (versions.Count == 0) return Missing($"phiên bản đã duyệt của {group.Key} có hiệu lực cho khoảng ngày đã yêu cầu ({string.Join(", ", group.Select(b => b.Content))})");
            if (versions.Count != 1 || covering.Count != 1)
                return new(AnswerStatus.NeedsClarification, "Khoảng áp dụng có thay đổi phiên bản, thiếu ngày bao phủ hoặc chồng lấn. Vui lòng chọn một ngày cụ thể; nếu vẫn chồng lấn, cần kiểm tra dữ liệu quy chế.", [], steps);
            foreach (var binding in group.Distinct())
            {
                var clauses = covering[0].Clauses.Where(c => c.Topic == binding.ClauseTopic).OrderBy(c => c.Id).ToList();
                steps.Add(new(group.Key, $"Chủ đề {PolicyCatalog.TopicLabel(binding.ClauseTopic)}: {clauses.Count} điều khoản trước lọc thời lượng nghỉ.", [covering[0].Id], clauses.Select(c => c.Id).ToArray()));
                if (clauses.Count == 0) return Missing(binding.Content);
                if (binding.ClauseTopic == "LeaveNotice")
                {
                    if (ClauseValidation.NoticeProblem(clauses) is { } problem)
                        return Missing($"các trường hợp báo trước nhất quán: {problem}");
                    if (question.LeaveDays.HasValue)
                    {
                        var last = question.LeaveDaysUnbounded ? (decimal?)null : question.LeaveDaysTo ?? question.LeaveDays;
                        clauses = clauses.Where(c => (last == null || c.MinimumLeaveDaysExclusive == null || last > c.MinimumLeaveDaysExclusive)
                            && (c.MaximumLeaveDaysInclusive == null || question.LeaveDays <= c.MaximumLeaveDaysInclusive)).ToList();
                    }
                    steps.Add(new(group.Key, question.LeaveDays.HasValue
                        ? $"Lọc số ngày nghỉ {question.LeaveDays}: lớn hơn cận dưới (nếu có), không vượt cận trên (nếu có). Còn {clauses.Count} điều khoản."
                        : "Không nêu thời lượng nghỉ: giữ tất cả trường hợp có căn cứ, không yêu cầu nhập thêm số ngày.",
                        [covering[0].Id], clauses.Select(c => c.Id).ToArray()));
                }
                if (clauses.Count == 0) return Missing($"{binding.Content} cho số ngày làm việc dự định nghỉ");
                sources.AddRange(clauses);
            }
        }
        var unique = sources.DistinctBy(c => c.Id).ToList();
        if (unique.Count == 0) return Missing("điều khoản trả lời ý hỏi");
        var response = string.Join("\n\n", unique.Select(c => c.Body));
        return new(AnswerStatus.Answered, response, unique, steps);
    }
}
