using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RestaurantInfrastructure;
using System.Globalization;  // ������ ��� CultureInfo

var builder = WebApplication.CreateBuilder(args);

// ������ ������ �� ����������.
builder.Services.AddControllersWithViews()
    .AddViewLocalization();  // ������ �������� ���������� ��� ������������

// ������������ ����������
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[]
    {
        new CultureInfo("uk-UA"),  // ��������� ��������
        new CultureInfo("en-US")   // ��������� �������� (�� �������)
    };

    options.DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("uk-UA");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

// ������������ ��������� ���� �����
builder.Services.AddDbContext<RestaurantDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// ������������ ������� ������� ������
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// ������ middleware ��� ����������
app.UseRequestLocalization();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();