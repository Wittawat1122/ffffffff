using Microsoft.EntityFrameworkCore;
using fffff.Models.Db; // 👈 namespace จาก scaffold

var builder = WebApplication.CreateBuilder(args);

// 👉 Add services
builder.Services.AddControllersWithViews();

// 👉 Add Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// 👉 เชื่อม Database (เหมือนในสไลด์)
builder.Services.AddDbContext<FinalProjectContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// 👉 Configure pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

// 👉 Route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
