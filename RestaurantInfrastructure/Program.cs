using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RestaurantInfrastructure;
using System.Globalization;  // Додаємо для CultureInfo

var builder = WebApplication.CreateBuilder(args);

// Додаємо сервіси до контейнера.
builder.Services.AddControllersWithViews()
    .AddViewLocalization();  // Додаємо підтримку локалізації для представлень

// Налаштування локалізації
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[]
    {
        new CultureInfo("uk-UA"),  // Українська культура
        new CultureInfo("en-US")   // Англійська культура (як запасна)
    };

    options.DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("uk-UA");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

// Налаштування контексту бази даних
builder.Services.AddDbContext<RestaurantDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Налаштування конвеєра обробки запитів
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Додаємо middleware для локалізації
app.UseRequestLocalization();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();