using TroLyQuyChe.Web.Services.Understanding;
namespace TroLyQuyChe.Web.Services;
public record PolicyBinding(string DocumentCode, string ClauseTopic, string Content);
public static class PolicyCatalog
{
    public const string TimeCode = "QC-THOIGIAN-LAMVIEC";
    public const string LeaveCode = "QC-DANGKY-NGHI";
    public const string OfficeCode = "QC-LAMVIEC-CONGTY";
    public static readonly IReadOnlyDictionary<PolicyIntent, PolicyBinding> Bindings = new Dictionary<PolicyIntent, PolicyBinding>
    {
        [PolicyIntent.WorkStart] = new(TimeCode, "WorkStart", "giờ bắt đầu làm việc"),
        [PolicyIntent.WorkEnd] = new(TimeCode, "WorkEnd", "giờ kết thúc làm việc"),
        [PolicyIntent.WorkSchedule] = new(TimeCode, "WorkSchedule", "lịch làm việc trong tuần"),
        [PolicyIntent.LunchBreak] = new(TimeCode, "LunchBreak", "nghỉ trưa"),
        [PolicyIntent.WorkDuration] = new(TimeCode, "WorkDuration", "số giờ làm việc mỗi ngày"),
        [PolicyIntent.LeaveWeekly] = new(LeaveCode, "LeaveWeekly", "giới hạn đăng ký nghỉ mỗi tuần"),
        [PolicyIntent.LeaveMonthly] = new(LeaveCode, "LeaveMonthly", "giới hạn đăng ký nghỉ mỗi tháng"),
        [PolicyIntent.LeaveException] = new(LeaveCode, "LeaveException", "nghỉ vượt giới hạn"),
        [PolicyIntent.LeaveNotice] = new(LeaveCode, "LeaveNotice", "thời hạn báo trước khi nghỉ"),
        [PolicyIntent.LeaveApproval] = new(LeaveCode, "LeaveApproval", "người duyệt yêu cầu nghỉ"),
        [PolicyIntent.LeaveRequest] = new(LeaveCode, "LeaveRequest", "cách đăng ký nghỉ"),
        [PolicyIntent.LeaveCalendar] = new(LeaveCode, "LeaveCalendar", "cách tính ngày nghỉ và tuần tháng"),
        [PolicyIntent.LateArrival] = new(OfficeCode, "LateArrival", "đi muộn"),
        [PolicyIntent.EarlyDeparture] = new(OfficeCode, "EarlyDeparture", "về sớm"),
        [PolicyIntent.LeavingOffice] = new(OfficeCode, "LeavingOffice", "ra ngoài trong giờ làm"),
        [PolicyIntent.Visitors] = new(OfficeCode, "Visitors", "tiếp khách tại công ty"),
        [PolicyIntent.MeetingRoom] = new(OfficeCode, "MeetingRoom", "sử dụng phòng họp"),
        [PolicyIntent.WorkplaceOrder] = new(OfficeCode, "WorkplaceOrder", "giữ trật tự"),
        [PolicyIntent.OfficeCleanliness] = new(OfficeCode, "OfficeCleanliness", "vệ sinh nơi làm việc"),
    };
    public static string TopicLabel(string topic) => Bindings.Values.FirstOrDefault(b => b.ClauseTopic == topic)?.Content is { } name
        ? char.ToUpperInvariant(name[0]) + name[1..] : topic;
    public static string IntentLabel(PolicyIntent intent) => TopicLabel(Bindings[intent].ClauseTopic);
    public static IReadOnlyList<string> Topics => Bindings.Values.Select(b => b.ClauseTopic).Distinct().ToArray();
    public static IReadOnlyList<string> TopicsFor(string code)
    {
        var topics = Bindings.Values.Where(b => b.DocumentCode == code).Select(b => b.ClauseTopic).Distinct().ToArray();
        return topics.Length > 0 ? topics : Topics;
    }
}
