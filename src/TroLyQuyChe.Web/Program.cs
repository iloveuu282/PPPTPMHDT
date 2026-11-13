using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using TroLyQuyChe.Web.Data;
using TroLyQuyChe.Web.Services;
using TroLyQuyChe.Web.Services.Understanding;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<ApplicationIdentity>(builder.Configuration.GetSection("Identity"));
builder.Services.AddRazorPages(o => o.Conventions.AuthorizeFolder("/Admin", "AdminOnly"));
builder.Services.AddAuthorization(o => o.AddPolicy("AdminOnly", p => p.RequireRole("Admin")));
builder.Services.AddScoped<VersionAdministrationService>();
builder.Services.AddScoped<AdminQueries>();
builder.Services.AddDbContext<RegulationDbContext>(o => o.UseSqlServer(builder.Configuration.GetConnectionString("Regulations")));
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<ApplicableDateResolver>();
builder.Services.AddScoped<QuestionAnswerService>();
builder.Services.AddScoped<QuestionRecognizer>();
builder.Services.AddScoped<PolicyLookupService>();
builder.Services.AddScoped<RegulationRepository>();
builder.Services.AddScoped<HistoryService>();
builder.Services.AddScoped<CitationSourceService>();
builder.Services.AddScoped<BrowserOwner>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(o =>
{
    o.Cookie.Name = "TroLyQuyChe.Owner";
    o.LoginPath = "/Account/Login";
    o.AccessDeniedPath = "/Account/Denied";
    o.Cookie.HttpOnly = true;
    o.Cookie.SameSite = SameSiteMode.Lax;
    o.ExpireTimeSpan = TimeSpan.FromDays(30);
    o.SlidingExpiration = true;
});
var app = builder.Build();
// Explicit setup command; normal startup never changes the schema.
if (args.Contains("--init-db") || args.Contains("--set-demo-admin"))
{
    var password = DemoUsers.RequireSetupPassword(builder.Configuration["DemoAdmin:Password"]);
    using var scope = app.Services.CreateScope();
    var setupDb = scope.ServiceProvider.GetRequiredService<RegulationDbContext>();
    var pending = (await setupDb.Database.GetPendingMigrationsAsync()).ToArray();
    var applied = await setupDb.Database.GetAppliedMigrationsAsync();
    if (pending.Any(m => m.EndsWith("_ReplaceWithSimpleOfficePolicies")) && applied.Any()
        && await setupDb.QaEntries.AnyAsync() && !args.Contains("--replace-policy-and-history"))
        throw new InvalidOperationException("Chưa áp dụng: thay bộ quy chế sẽ xóa lịch sử và trích dẫn cũ. Cần bản sao lưu và xác nhận rõ ràng, sau đó mới dùng --replace-policy-and-history cùng --init-db.");
    await setupDb.Database.MigrateAsync();
    if (args.Contains("--set-demo-admin")) await DemoUsers.SetAdminPasswordAsync(setupDb, password);
    else await DemoUsers.InitializeAsync(setupDb, password);
    Console.WriteLine("TroLyQuyCheDb: migration và seed hoàn tất.");
    return;
}
app.UseExceptionHandler("/Error");
// Serve wwwroot directly in both Development and Production. CSS URLs use
// asp-append-version, so a rebuilt stylesheet also invalidates the browser cache.
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();
app.Run();
