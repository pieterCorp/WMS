using Backend.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// JSON serializer options (preserve PascalCase for compatibility with tests)
builder.Services.AddControllers().AddJsonOptions(options => { options.JsonSerializerOptions.PropertyNamingPolicy = null; options.JsonSerializerOptions.PropertyNameCaseInsensitive = true; });

// Determine environment early from environment variable so tests can set it before WebApplicationFactory runs
var envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? builder.Environment.EnvironmentName;

if (envName != "Testing")
{
    builder.Services.AddEndpointsApiExplorer();
}

// Business services (register by interface)
builder.Services.AddScoped<Backend.Business.Services.IOrderService, Backend.Business.Services.OrderService>();
builder.Services.AddScoped<Backend.Business.Services.IPickService, Backend.Business.Services.PickService>();
builder.Services.AddScoped<Backend.Business.Services.IInventoryService, Backend.Business.Services.InventoryService>();

// DbContext registration
if (envName == "Testing")
{
    builder.Services.AddDbContext<WarehouseDbContext>(options => options.UseInMemoryDatabase("WarehouseTestDb"));
}
else
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Server=(localdb)\\mssqllocaldb;Database=WarehouseMvp;Trusted_Connection=true;";
    builder.Services.AddDbContext<WarehouseDbContext>(options => options.UseSqlServer(connectionString));
}

var app = builder.Build();

// Seed and migrate when not in testing
if (envName != "Testing")
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<WarehouseDbContext>();
    db.Database.Migrate();
    Backend.Data.DbSeeder.Seed(db);
}

if (envName != "Testing")
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
