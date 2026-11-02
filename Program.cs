using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AlmaDelMarPampeano.Data;
using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Localization;
using AlmaDelMarPampeano.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// RequireConfirmedAccount = false: no hay servicio de mail, así que los usuarios
// nuevos pueden ingresar apenas se registran.
// AddUserManager<AppUserManager>: cada usuario que se registra recibe el rol "Cliente".
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddUserManager<AppUserManager>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddErrorDescriber<DescriptorErroresIdentity>();

builder.Services.AddHttpContextAccessor(); // lo usa ApplicationDbContext para saber qué usuario hace cada cambio
builder.Services.AddControllersWithViews();

// Cliente HTTP tipado para el asistente de IA (Google Gemini)
builder.Services.AddHttpClient<GeminiService>(cliente =>
{
    cliente.BaseAddress = new Uri("https://generativelanguage.googleapis.com/");
    cliente.Timeout = TimeSpan.FromSeconds(30);
});

// Límite de consultas al asistente: 8 por minuto por visitante (protege la cuota gratuita de Gemini)
builder.Services.AddRateLimiter(opciones =>
{
    opciones.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    opciones.AddPolicy("chat", contexto =>
        RateLimitPartition.GetFixedWindowLimiter(
            contexto.Connection.RemoteIpAddress?.ToString() ?? "desconocida",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 8,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});

var app = builder.Build();

// Cultura fija es-AR: coma decimal y formatos argentinos, en cualquier servidor
var culturaAr = new CultureInfo("es-AR");
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(culturaAr),
    SupportedCultures = new List<CultureInfo> { culturaAr },
    SupportedUICultures = new List<CultureInfo> { culturaAr }
});

// --- INICIO: migraciones + datos iniciales (roles, usuarios de prueba, estados, tipos de luz, colecciones) ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();          // crea/actualiza la base si hace falta
        await DbInitializer.Initialize(services);  // roles, usuarios de prueba y datos iniciales
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error al inicializar la base de datos.");
    }
}
// --- FIN ---

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

app.Run();