using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RestaurantDomain.Models;
using RestaurantInfrastructure;

var builder = WebApplication.CreateBuilder(args);

// Додаємо DbContext
builder.Services.AddDbContext<RestaurantDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Додаємо Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<RestaurantDbContext>()
    .AddDefaultTokenProviders();

// Налаштування автентифікації та авторизації
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

// Додаємо контролери та представлення
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Налаштування обробки помилок
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Додаємо автентифікацію та авторизацію в пайплайн
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Налаштування ролей і користувачів
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    // Створюємо ролі
    string[] roleNames = { "Admin", "Chef", "User" };
    foreach (var roleName in roleNames)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    // Створюємо адміністратора
    var adminEmail = "admin@restaurant.com";
    var adminPassword = "Admin123!";
    if (await userManager.FindByEmailAsync(adminEmail) == null)
    {
        var adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FullName = "Admin User"
        };
        var result = await userManager.CreateAsync(adminUser, adminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }

    // Створюємо шеф-кухаря
    var chefEmail = "chef@restaurant.com";
    var chefPassword = "Chef123!";
    if (await userManager.FindByEmailAsync(chefEmail) == null)
    {
        var chefUser = new ApplicationUser
        {
            UserName = chefEmail,
            Email = chefEmail,
            FullName = "Chef User"
        };
        var result = await userManager.CreateAsync(chefUser, chefPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(chefUser, "Chef");
        }
    }

    // Звичайний користувач створюється через реєстрацію, тому не додаємо його тут
}

app.Run();