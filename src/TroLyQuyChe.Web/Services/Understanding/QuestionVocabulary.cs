namespace TroLyQuyChe.Web.Services.Understanding;

public enum QuestionTopic { Unknown, WorkingHours, Leave, Office }
public enum PolicyIntent
{
    WorkStart, WorkEnd, WorkSchedule, LunchBreak, WorkDuration,
    LeaveWeekly, LeaveMonthly, LeaveException, LeaveNotice, LeaveApproval, LeaveRequest, LeaveCalendar,
    LateArrival, EarlyDeparture, LeavingOffice, Visitors, MeetingRoom, WorkplaceOrder, OfficeCleanliness
}
public record RecognizedQuestion(QuestionTopic Topic, IReadOnlyList<PolicyIntent> Intents,
    decimal? LeaveDays = null, string? Clarification = null, string? MissingEvidence = null,
    decimal? LeaveDaysTo = null, bool LeaveDaysUnbounded = false);

// Chữ thường không dấu. Mở rộng ý định bằng mẫu câu, không thêm đáp án vào đây.
public static class QuestionVocabulary
{
    public static readonly IReadOnlyDictionary<PolicyIntent, string[]> Patterns = new Dictionary<PolicyIntent, string[]>
    {
        [PolicyIntent.WorkStart] = [@"\b(?:bat dau|vao lam|vao ca|di lam luc|den cong ty luc|gio vao)\b"],
        [PolicyIntent.WorkEnd] = [@"\b(?:tan lam|tan ca|ket thuc|gio ve|may gio.*ve|ve luc may gio)\b"],
        [PolicyIntent.WorkSchedule] = [@"\b(?:lich (?:di )?lam|lich trong tuan|lam.*thu may|lam.*ngay nao|lam.*(?:may|bao nhieu) ngay|cac ngay lam viec|ngay nghi.*tuan|cuoi tuan|thu (?:hai|ba|tu|nam|sau|bay|2|3|4|5|6|7)|chu nhat)\b"],
        [PolicyIntent.LunchBreak] = [@"\b(?:nghi trua|an trua|gio trua)\b"],
        [PolicyIntent.WorkDuration] = [@"\b(?:may tieng|bao nhieu tieng|bao nhieu gio|so gio lam|thoi luong lam)\b"],
        [PolicyIntent.LateArrival] = [@"\b(?:di muon|den muon|vao muon|tre gio|di tre)\b"],
        [PolicyIntent.EarlyDeparture] = [@"\b(?:ve som|tan ca som|roi cong ty som)\b"],
        [PolicyIntent.LeavingOffice] = [@"\b(?:ra ngoai|ra khoi cong ty|roi van phong)\b"],
        [PolicyIntent.Visitors] = [@"\b(?:khach|nguoi than|ban be.*cong ty)\b"],
        [PolicyIntent.MeetingRoom] = [@"\b(?:phong hop|dat phong|dang ky hop)\b"],
        [PolicyIntent.WorkplaceOrder] = [@"\b(?:on ao|lam on|noi to|trat tu|tieng on|mo nhac|noi chuyen lon)\b"],
        [PolicyIntent.OfficeCleanliness] = [@"\b(?:ve sinh|bo rac|don dep|sach se|cho lam.*sach)\b"]
    };
    public static readonly string[] LeaveAliases = ["nghi phep", "xin nghi", "xin vang", "vang co phep", "dang ky nghi", "nghi", "phep nam"];
    public static readonly string[] NoticePatterns = [@"\b(?:bao|thong bao|xin phep|gui(?: yeu cau| don)?|nop(?: don)?)\s+truoc\b", @"\btruoc\s+(?:bao lau|bao nhieu|may)\b"];
    public static readonly string[] ApprovalPatterns = [@"\b(?:ai.*duyet|ai.*dong y|ai.*cho phep|cap nao.*duyet|nguoi duyet|can.*duyet|phai.*duyet)\b"];
    public static readonly string[] AllowancePatterns = [@"\b(?:bao nhieu|may)\s+(?:ngay|hom)\b", @"\b(?:toi da|han muc|gioi han|so ngay|duoc nghi|duoc xin)\b"];
    public const string NoticeQuestionSpanPattern = @"\b(?:bao|thong bao|xin phep|gui(?: yeu cau| don)?|nop(?: don)?)\s+truoc\b.*?(?=\b(?:va|con)\b|[,?;]|$)|\btruoc\s+(?:bao lau|bao nhieu|may)\b.*?(?=\b(?:va|con)\b|[,?;]|$)";
    public static readonly IReadOnlyDictionary<string, decimal> LeaveDurationWords = new Dictionary<string, decimal>
    { ["mot"] = 1, ["hai"] = 2, ["ba"] = 3, ["bon"] = 4, ["nam"] = 5, ["sau"] = 6, ["bay"] = 7, ["tam"] = 8, ["chin"] = 9, ["muoi"] = 10 };
    public static readonly (string Pattern, string Content)[] MissingPatterns =
    [
        (@"\b(?:lam viec (?:tai nha|o nha|tu xa)|lam o nha|wfh|remote|work from home)\b", "làm việc tại nhà hoặc làm việc từ xa; phạm vi hiện tại chỉ gồm thời gian làm việc, đăng ký nghỉ và làm việc tại công ty"),
        (@"\b(?:laptop|may tinh|thiet bi|ban giao|vpn)\b", "sử dụng hoặc bàn giao thiết bị"),
        (@"\b(?:ngay le|le tet|tet|tang ca|lam them gio|thu viec)\b", "ngày lễ, làm thêm giờ hoặc thử việc"),
        (@"\b(?:di muon|den muon|ve som).*\b(?:bao nhieu|may)\s*(?:phut|gio)\b", "thời lượng đi muộn hoặc về sớm được cho phép"),
        (@"\b(?:luong|thuong|tru tien|tru cong|phat|boi thuong)\b", "tiền lương, chấm công hoặc xử phạt"),
        (@"\b(?:bao hiem|phu cap|tro cap|internet)\b", "bảo hiểm hoặc phụ cấp"),
        (@"\b(?:phep nam|phep ton|chuyen phep|ket hon|thai san|om dau|tang le|chua du 12 thang)\b", "phép năm hoặc loại nghỉ đặc biệt; không suy từ giới hạn đăng ký nghỉ tuần và tháng"),
        (@"\bnghi\b.*\b(?:moi|mot|1|hang|ca|trong)\s+nam\b(?!\s*\d)", "hạn mức nghỉ theo năm; không suy từ giới hạn đăng ký nghỉ tuần và tháng"),
        (@"\b(?:con (?:duoc )?nghi|con bao nhieu|da nghi|lich su nghi cua toi|tuan nay toi.*nghi duoc|thang nay toi.*nghi duoc)\b", "số ngày nghỉ thực tế của cá nhân; hệ thống tra quy định, chưa quản lý đơn và số dư nghỉ")
    ];
}
