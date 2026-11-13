using System.Text.RegularExpressions;
namespace TroLyQuyChe.Web.Services.Understanding;

public class QuestionRecognizer
{
    private static bool Any(string q, IEnumerable<string> patterns) => patterns.Any(p => Regex.IsMatch(q, p));
    public RecognizedQuestion Recognize(string question)
    {
        var q = Regex.Replace(ApplicableDateResolver.Normalize(question), @"\s+", " ").Trim();
        // Preserve the distinction between the polite “làm ơn” and “làm ồn” before removing accents.
        var polite = Regex.Replace(question.ToLowerInvariant(), @"\blàm ơn\b", " ");
        q = Regex.Replace(ApplicableDateResolver.Normalize(polite), @"\s+", " ").Trim();
        q = Regex.Replace(q, @"\blam on\s+(?=cho biet|cho toi|giup|hay)\b", " ");
        // Explicitly excluded clauses do not become positive intents. Other negation needs clarification.
        q = Regex.Replace(q, @"\bkhong\s+(?:hoi|muon hoi|can biet)\b[^,;?]*(?:[,;]|$)", " ");
        if (Regex.IsMatch(q, @"\b(?:khong phai|dung tra loi|khong muon)\b"))
            return new(QuestionTopic.Unknown, [], Clarification: "Bạn vui lòng viết riêng nội dung muốn tra cứu, không kèm phần loại trừ.");
        var missing = QuestionVocabulary.MissingPatterns.Where(p => Regex.IsMatch(q, p.Pattern)).Select(p => p.Content).Distinct().ToArray();
        if (missing.Length > 0) return new(QuestionTopic.Unknown, [], MissingEvidence: string.Join("; ", missing));
        bool Match(PolicyIntent i) => Any(q, QuestionVocabulary.Patterns[i]);
        var office = new[] { PolicyIntent.LateArrival, PolicyIntent.EarlyDeparture, PolicyIntent.LeavingOffice,
            PolicyIntent.Visitors, PolicyIntent.MeetingRoom, PolicyIntent.WorkplaceOrder, PolicyIntent.OfficeCleanliness }.Where(Match).ToArray();
        var hours = new[] { PolicyIntent.WorkStart, PolicyIntent.WorkEnd, PolicyIntent.WorkSchedule,
            PolicyIntent.LunchBreak, PolicyIntent.WorkDuration }.Where(Match).ToArray();
        if (office.Contains(PolicyIntent.EarlyDeparture))
            hours = hours.Where(i => i != PolicyIntent.WorkEnd).ToArray();
        // The start time is contextual in a late-arrival question; it is not a second request.
        if (office.Contains(PolicyIntent.LateArrival) && Regex.IsMatch(q, @"\bbao truoc gio bat dau\b"))
            hours = hours.Where(i => i != PolicyIntent.WorkStart).ToArray();
        var leave = QuestionVocabulary.LeaveAliases.Any(a => Regex.IsMatch(q, @"\b" + Regex.Escape(a) + @"\b"));
        var explicitLeave = Regex.IsMatch(q, @"\b(?:xin nghi|nghi phep|xin vang|dang ky nghi)\b")
            || (leave && Any(q, QuestionVocabulary.NoticePatterns));
        // Nghỉ trưa/ngày nghỉ cuối tuần không phải yêu cầu nghỉ phép.
        if (hours.Length > 0 && !explicitLeave) leave = false;
        if (office.Length > 0 && !explicitLeave) leave = false;
        if (leave) hours = hours.Where(i => i != PolicyIntent.WorkSchedule).ToArray();
        var groups = (office.Length > 0 ? 1 : 0) + (hours.Length > 0 ? 1 : 0) + (leave ? 1 : 0);
        if (groups > 1) return new(QuestionTopic.Unknown, [], Clarification: "Bạn vui lòng tách câu hỏi theo từng nội dung: thời gian làm việc, đăng ký nghỉ hoặc làm việc tại công ty.");
        if (office.Length > 0) return new(QuestionTopic.Office, office);
        if (hours.Length > 0) return new(QuestionTopic.WorkingHours, hours);
        if (!leave)
        {
            if (Regex.IsMatch(q, @"\b(?:gio lam|thoi gian lam|gio hanh chinh)\b"))
                return new(QuestionTopic.WorkingHours, [], Clarification: "Bạn hỏi giờ bắt đầu, giờ kết thúc, nghỉ trưa hay lịch làm việc trong tuần?");
            if (Regex.IsMatch(q, @"\b(?:lam viec (?:trong|tai) cong ty|noi quy cong ty)\b"))
                return new(QuestionTopic.Office, [], Clarification: "Bạn hỏi đi muộn, về sớm, ra ngoài, tiếp khách, phòng họp, trật tự hay vệ sinh?");
            return new(QuestionTopic.Unknown, [], MissingEvidence: "chủ đề được hỏi. Kho hiện có thời gian làm việc, đăng ký nghỉ và làm việc tại công ty");
        }
        var notice = Any(q, QuestionVocabulary.NoticePatterns);
        var approval = Any(q, QuestionVocabulary.ApprovalPatterns);
        var exception = Regex.IsMatch(q, @"\b(?:vuot|them|qua han muc|qua\s+\d+|hon gioi han|ngoai han muc)\b");
        var request = Regex.IsMatch(q, @"\b(?:thu tuc|dang ky.*(?:the nao|o dau)|gui.*cho ai|nop don|gui don|lam gi|ghi.*gi)\b");
        var allowance = Any(Regex.Replace(q, QuestionVocabulary.NoticeQuestionSpanPattern, " "), QuestionVocabulary.AllowancePatterns);
        if (notice) allowance = Regex.IsMatch(Regex.Replace(q, QuestionVocabulary.NoticeQuestionSpanPattern, " "), @"\b(?:(?:bao nhieu|may)\s+(?:ngay|hom)|han muc|toi da)\b");
        var duration = LeaveDurationParser.Parse(q);
        if (duration.Error is not null) return new(QuestionTopic.Leave, [], Clarification: duration.Error);
        var week = Regex.IsMatch(q, @"\b(?:tuan|7 ngay)\b");
        var month = Regex.IsMatch(q, @"\bthang\b");
        if (duration.From.HasValue && (week || month) && !notice && !approval) allowance = true;
        var calendar = Regex.IsMatch(q, @"\b(?:tinh.*(?:ngay|tuan|thang)|ngay lam viec hay|ngay lich|tuan.*bat dau|(?:tuan|thang).*tinh)\b");
        if (calendar) request = false;
        var intents = new List<PolicyIntent>();
        if (notice) intents.Add(PolicyIntent.LeaveNotice);
        if (exception) intents.Add(PolicyIntent.LeaveException);
        if (approval && !exception) intents.Add(PolicyIntent.LeaveApproval);
        // Numeric requests cannot be declared “within limit” without knowing personal usage.
        // Present the applicable limits and both approval conditions from SQL.
        if (approval && duration.From.HasValue)
        {
            if (week) intents.Add(PolicyIntent.LeaveWeekly);
            if (month) intents.Add(PolicyIntent.LeaveMonthly);
            intents.Add(PolicyIntent.LeaveException);
        }
        if (request && !notice && !exception) intents.Add(PolicyIntent.LeaveRequest);
        if (calendar && !notice) intents.Add(PolicyIntent.LeaveCalendar);
        if (allowance && !exception && !request && !calendar)
        {
            if (!month || week) intents.Add(PolicyIntent.LeaveWeekly);
            if (!week || month) intents.Add(PolicyIntent.LeaveMonthly);
            intents.Add(PolicyIntent.LeaveException);
        }
        if (intents.Count == 0) return new(QuestionTopic.Leave, [], Clarification: "Bạn hỏi được nghỉ bao nhiêu ngày, phải báo trước bao lâu hay ai duyệt?");
        return new(QuestionTopic.Leave, intents.Distinct().ToArray(),
            notice || approval ? duration.From : null, LeaveDaysTo: duration.To, LeaveDaysUnbounded: duration.Unbounded);
    }
}
