using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TroLyQuyChe.Web.Data;
using TroLyQuyChe.Web.Services;

var config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("src/TroLyQuyChe.Web/appsettings.json").AddEnvironmentVariables().Build();
Directory.CreateDirectory("output");
var json = new JsonSerializerOptions { WriteIndented = true };
if (args.Contains("--prepare-final-demo"))
{
    var password = DemoUsers.RequireSetupPassword(config["DemoAdmin:Password"]);
    var connection = new SqlConnectionStringBuilder(config.GetConnectionString("Regulations"))
        { InitialCatalog = "TroLyQuyCheDemo_Final_" + Guid.NewGuid().ToString("N")[..8] };
    await using var db = new RegulationDbContext(new DbContextOptionsBuilder<RegulationDbContext>().UseSqlServer(connection.ConnectionString).Options);
    await db.Database.MigrateAsync();
    await DemoUsers.InitializeAsync(db, password);
    var version = await FinalizeDemoData.CompleteTimeAsync(db);
    await File.WriteAllTextAsync("output/final-demo.json", JsonSerializer.Serialize(new { ConnectionString = connection.ConnectionString, Database = connection.InitialCatalog, TimeVersionId = version }, json));
    Console.WriteLine("FINAL DEMO: " + connection.InitialCatalog); return;
}
if (args.Contains("--prepare-demo"))
{
    var password = DemoUsers.RequireSetupPassword(config["DemoAdmin:Password"]);
    var connection = new SqlConnectionStringBuilder(config.GetConnectionString("Regulations"))
        { InitialCatalog = "TroLyQuyCheDemo_Simple_" + Guid.NewGuid().ToString("N")[..8] };
    await using var db = new RegulationDbContext(new DbContextOptionsBuilder<RegulationDbContext>().UseSqlServer(connection.ConnectionString).Options);
    await db.Database.MigrateAsync();
    await DemoUsers.InitializeAsync(db, password);
    await File.WriteAllTextAsync("output/simple-demo.json", JsonSerializer.Serialize(new { ConnectionString = connection.ConnectionString, Database = connection.InitialCatalog }, json));
    Console.WriteLine("FRESH DEMO: " + connection.InitialCatalog); return;
}
var profile = JsonDocument.Parse(await File.ReadAllTextAsync("output/simple-demo.json"));
var testConnection = profile.RootElement.GetProperty("ConnectionString").GetString()!;
if (!new SqlConnectionStringBuilder(testConnection).InitialCatalog.StartsWith("TroLyQuyCheDemo_Simple_"))
    throw new Exception("Kiểm thử chỉ chạy trên database riêng.");
await FinalRegressionChecks.QueriesAsync(testConnection);
await SimplePolicyChecks.RunAsync(testConnection);
await FinalRegressionChecks.AdminAsync(testConnection);
