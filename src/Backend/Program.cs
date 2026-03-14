using Backend.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers().AddJsonOptions(options => { options.JsonSerializerOptions.PropertyNamingPolicy = null; options.JsonSerializerOptions.PropertyNameCaseInsensitive = true; });
builder.Services.AddOpenApi();
builder.Services.AddScoped<Backend.Services.PickService>();
builder.Services.AddScoped<Backend.Services.InventoryService>();
builder.Services.AddScoped<Backend.Services.OrderService>();

// Register DbContext - use InMemory for Testing, SqlServer for others
if (builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddDbContext<WarehouseDbContext>(options =>
        options.UseInMemoryDatabase("WarehouseTestDb"));
}
else
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? "Server=(localdb)\\mssqllocaldb;Database=WarehouseMvp;Trusted_Connection=true;";
    builder.Services.AddDbContext<WarehouseDbContext>(options =>
        options.UseSqlServer(connectionString));
}

var app = builder.Build();

// Seed database with test data when running (skip during Testing)
if (!app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<WarehouseDbContext>();
    // Apply any pending migrations
    db.Database.Migrate();
    // Seed sample data for development
    Backend.Data.DbSeeder.Seed(db);
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
