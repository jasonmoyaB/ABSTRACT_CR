using Microsoft.EntityFrameworkCore;
using Abstract_CR.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Abstract_CR.Helpers;
using Abstract_CR.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});
builder.Services.AddHttpContextAccessor();


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
    options.IdleTimeout = TimeSpan.FromHours(1);
    options.Cookie.Name = ".AbstractCR.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// inyeccion de dependencias
builder.Services.AddScoped<UserHelper>();
builder.Services.AddScoped<EbooksHelper>();
builder.Services.AddScoped<SuscripcionesHelper>();
builder.Services.AddScoped<CometarioRecetaHelper>();
builder.Services.AddScoped<InteraccionHelper>();
builder.Services.AddScoped<RecetasHelper>();
builder.Services.AddScoped<MenuSemanalHelper>();

//  email
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddHostedService<VencimientosNotifier>();

// Servicio de  PDF para reportes
builder.Services.AddScoped<Abstract_CR.Services.IReportePdfService, Abstract_CR.Services.ReportePdfService>();

var app = builder.Build();

app.UseForwardedHeaders();

// CÓDIGO TEMPORAL PARA PROBAR LA CONEXIÓN
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
       await context.Database.MigrateAsync();
        Console.WriteLine("Migraciones de base de datos aplicadas correctamente.");
        
        await context.Database.OpenConnectionAsync();
        Console.WriteLine("Conexión exitosa a la base de datos de Azure!");
        await context.Database.CloseConnectionAsync();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error al aplicar migraciones: {ex.Message}");
        try
        {
            await context.Database.OpenConnectionAsync();
            Console.WriteLine("Conexión exitosa a la base de datos de Azure!");
            await context.Database.CloseConnectionAsync();
        }
        catch (Exception innerEx)
        {
            Console.WriteLine($"Error de conexión: {innerEx.Message}");
        }
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
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
