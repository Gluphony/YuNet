using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// 1. CONFIGURACIÓN DE AUTENTICACIÓN
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/Index"; // A dónde ir si no están logueados
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60); // Duración de la sesión
        options.AccessDeniedPath = "/Home/AccesoDenegado"; // A dónde ir si no tienen permisos
    });

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

// 2. MIDDLEWARES DE SEGURIDAD (El orden aquí es estricto e importante)
app.UseAuthentication(); // Primero validamos QUIÉN eres
app.UseAuthorization();  // Segundo validamos QUÉ PUEDES hacer (Tenías esta línea duplicada)

app.MapControllerRoute(
    name: "default",
    // PRO-TIP: Cambié "Home" por "Login" para que el sistema abra el login por defecto.
    pattern: "{controller=Login}/{action=Index}/{id?}");

app.Run();