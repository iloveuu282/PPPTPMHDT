using System.Text.Json;
using TroLyQuyChe.Web.Services.Understanding;

namespace TroLyQuyChe.Web.Services;

// Stored with QaEntry and its citations in the same transaction.
public record ProcessingTrace(
    int SchemaVersion, string RuleSet, DateTimeOffset CapturedAt, DateOnly ServerToday,
    string NormalizedQuestion, DateOnly? SelectedDate, DateOnly? ResolvedFrom, DateOnly? ResolvedTo,
    string Topic, string[] Intents, decimal? LeaveDays, string[] RequiredDocuments,
    string? DateClarification, string? IntentClarification, string? MissingEvidence,
    LookupStep[] LookupSteps, int[] SourceClauseIds)
{
    public static ProcessingTrace Capture(string question, DateOnly? selected, DateResolution date,
        RecognizedQuestion? recognized, PolicyAnswer answer, DateTimeOffset now) => new(
        1, "csharp-rules-2026.11-final-v2", now,
        DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeBySystemTimeZoneId(now, "SE Asia Standard Time").DateTime),
        ApplicableDateResolver.Normalize(question), selected, date.From, date.To,
        recognized?.Topic.ToString() ?? "NotProcessed",
        recognized?.Intents.Select(PolicyCatalog.IntentLabel).ToArray() ?? [], recognized?.LeaveDays,
        recognized?.Intents.Select(i => PolicyCatalog.Bindings[i].DocumentCode).Distinct().ToArray() ?? [],
        date.Clarification, recognized?.Clarification, recognized?.MissingEvidence,
        answer.Steps?.ToArray() ?? [], answer.Sources.Select(c => c.Id).ToArray());

    public string ToJson() => JsonSerializer.Serialize(this);
    public static ProcessingTrace? Read(string? json)
    {
        if (string.IsNullOrEmpty(json)) return null;
        try
        {
            var trace = JsonSerializer.Deserialize<ProcessingTrace>(json);
            if (trace?.SchemaVersion != 1 || string.IsNullOrWhiteSpace(trace.RuleSet)
                || trace.NormalizedQuestion is null || trace.Topic is null || trace.Intents is null
                || trace.RequiredDocuments is null || trace.SourceClauseIds is null || trace.LookupSteps is null
                || trace.LookupSteps.Any(s => s is null || s.Description is null || s.DocumentCode is null
                    || s.VersionIds is null || s.ClauseIds is null)) return null;
            return trace;
        }
        catch (JsonException) { return null; }
    }
    public string TopicLabel => Topic switch
    {
        "RemoteWork" => "Làm việc tại nhà", "Leave" => "Nghỉ phép",
        "Office" => "Làm việc tại công ty", "WorkingHours" => "Giờ làm việc",
        "NotProcessed" => "Chưa nhận diện do cần làm rõ thời điểm", _ => "Chưa xác định chủ đề"
    };
}
