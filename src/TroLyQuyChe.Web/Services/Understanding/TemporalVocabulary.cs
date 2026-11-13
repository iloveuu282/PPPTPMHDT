namespace TroLyQuyChe.Web.Services.Understanding;

/// <summary>Các dấu hiệu yêu cầu tra lịch sử nhưng chưa đủ mốc để chọn phiên bản.</summary>
public static class TemporalVocabulary
{
    // Không dùng riêng từ 'trước': 'báo trước' là ý hỏi thời hạn, không phải tra quá khứ.
    public static readonly string[] UnspecifiedHistoricalPatterns = [
        @"\b(?:qua khu|truoc day|truoc kia|ngay truoc|thoi truoc|hoi truoc|khi truoc|ngay xua|hoi xua)\b",
        @"\b(?:quy che|quy dinh|phien ban|ban)\s+cu\b",
        @"\b(?:hom do|luc do|thoi diem do|da tung)\b"
    ];
}
