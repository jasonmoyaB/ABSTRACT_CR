using Microsoft.EntityFrameworkCore;
using Abstract_CR.Data;
using Microsoft.AspNetCore.Http.Features;
using Abstract_CR.Helpers;
using Abstract_CR.Services;

var builder = WebApplication.CreateBuilder(args);

// Configurar límites de archivo
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10MB
});

// Se agrega el DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configurar sesiones
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Registrar Helpers para inyeccion de dependencias
builder.Services.AddScoped<UserHelper>();

// Configurar servicios de email
builder.Services.AddScoped<IEmailService, EmailService>();

var app = builder.Build();

//

// C�DIGO TEMPORAL PARA PROBAR LA CONEXION
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        // Intenta conectar y hacer una consulta simple
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
