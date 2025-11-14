using Microsoft.EntityFrameworkCore;
using Abstract_CR.Data;
using Microsoft.AspNetCore.Http.Features;
using Abstract_CR.Helpers;
using Abstract_CR.Services;

var builder = WebApplication.CreateBuilder(args);


builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10 * 1024 * 1024; 
});

// Se agrega el DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddControllersWithViews();


builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// inyeccion de dependencias
builder.Services.AddScoped<UserHelper>();
builder.Services.AddScoped<EbooksHelper>();
builder.Services.AddScoped<SuscripcionesHelper>();
builder.Services.AddScoped<CometarioRecetaHelper>();
builder.Services.AddScoped<InteraccionHelper>();
builder.Services.AddScoped<RecetasHelper>();

//  email
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddHostedService<VencimientosNotifier>();

// Servicio de  PDF para reportes
builder.Services.AddScoped<Abstract_CR.Services.IReportePdfService, Abstract_CR.Services.ReportePdfService>();

var app = builder.Build();

//


using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        
        await context.Database.OpenConnectionAsync();
        Console.WriteLine("Conexi�n exitosa a la base de datos de Azure!");
        await context.Database.CloseConnectionAsync();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error de conexi�n: {ex.Message}");
    }
}

//

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Usar sesiones
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
