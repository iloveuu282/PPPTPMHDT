using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TroLyQuyChe.Web.Data;

namespace TroLyQuyChe.Web.Services;

public static class DemoUsers
{
    public static string RequireSetupPassword(string? password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new InvalidOperationException("Cần đặt biến môi trường DemoAdmin__Password trước khi khởi tạo tài khoản. Không ghi mật khẩu vào mã nguồn.");
        return password;
    }
    // Explicit maintenance command only; normal startup never changes credentials.
    public static async Task SetAdminPasswordAsync(RegulationDbContext db, string password)
    {
        await InitializeAsync(db, password);
        var user = await db.AppUsers.SingleAsync(u => u.Username == "admin");
        user.PasswordHash = new PasswordHasher<AppUser>().HashPassword(user, password);
        user.Role = "Admin"; user.IsActive = true;
        await db.SaveChangesAsync();
    }
    public static async Task InitializeAsync(RegulationDbContext db, string password)
    {
        if (await db.AppUsers.AnyAsync(u => u.Username == "admin")) return;
        var user = new AppUser { Username = "admin", Role = "Admin" };
        user.PasswordHash = new PasswordHasher<AppUser>().HashPassword(user, password);
        db.AppUsers.Add(user);
        await db.SaveChangesAsync();
    }
}
