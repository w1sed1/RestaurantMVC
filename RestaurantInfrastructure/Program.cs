using Microsoft.EntityFrameworkCore;
using RestaurantInfrastructure;

var builder = WebApplication.CreateBuilder(args);

// Додаємо контролери та представлення
builder.Services.AddControllersWithViews();

// Реєструємо RestaurantDbContext у контейнері залежностей
builder.Services.AddDbContext<RestaurantDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();