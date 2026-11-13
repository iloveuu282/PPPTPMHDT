using TroLyQuyChe.Web.Data;
using TroLyQuyChe.Web.Services.Understanding;

namespace TroLyQuyChe.Web.Services;

public class QuestionAnswerService(RegulationRepository repository, ApplicableDateResolver dates,
    QuestionRecognizer recognizer, PolicyLookupService policies, TimeProvider clock)
{
    public async Task<long> AskAsync(string question, DateOnly? selected, string owner)
    {
        if (string.IsNullOrWhiteSpace(question) || question.Trim().Length > 2000)
            throw new ArgumentException("Câu hỏi cần từ 1 đến 2000 ký tự.");
        var date = dates.Resolve(question, selected);
        // Capture decisions at processing time; never reconstruct old traces using new rules.
        var recognized = date.Clarification is null ? recognizer.Recognize(question) : null;
        var answer = date.Clarification is not null
            ? new PolicyAnswer(AnswerStatus.NeedsClarification, date.Clarification, [])
            : await policies.LookupAsync(recognized!, date.From!.Value, date.To!.Value);
        var entry = new QaEntry
        {
            Question = question.Trim(), OwnerId = owner, ApplicableFrom = date.From, ApplicableTo = date.To,
            AskedAt = clock.GetUtcNow(), Status = answer.Status, Response = answer.Response,
            ProcessingTraceJson = ProcessingTrace.Capture(question, selected, date, recognized, answer, clock.GetUtcNow()).ToJson()
        };
        if (answer.Status == AnswerStatus.Answered)
            entry.Citations = answer.Sources.Select(c => new Citation { ClauseId = c.Id }).ToList();
        return await repository.SaveAnswerAsync(entry);
    }
}
