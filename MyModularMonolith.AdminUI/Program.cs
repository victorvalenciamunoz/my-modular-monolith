using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using MyModularMonolith.AdminUI;
using MyModularMonolith.AdminUI.Authentication;
using MyModularMonolith.AdminUI.Authentication.Handlers;
using MyModularMonolith.AdminUI.Authentication.Services;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

// Necesario para acceder a HttpContext en servicios
builder.Services.AddHttpContextAccessor();

// Registrar servicios de autenticación como Singleton para compartir sesiones
builder.Services.AddSingleton<IServerSessionService, ServerSessionService>();
builder.Services.AddSingleton<ITokenService, TokenService>();

// Registrar servicio de cookies de sesión como Scoped (por circuito de Blazor)
builder.Services.AddScoped<ISessionCookieService, SessionCookieService>();

// Registrar el handler para agregar tokens automáticamente
builder.Services.AddTransient<AuthenticatedHttpClientHandler>();

// Configurar HttpClient para backend SIN el handler (para evitar recursión en refresh)
builder.Services.AddHttpClient("BackendClient", x => x.BaseAddress = new Uri("https+http://api"));

// Configurar HttpClient principal CON el handler autenticado
builder.Services.AddHttpClient<BackendHttpClient>(x => x.BaseAddress = new Uri("https+http://api"))
    .AddHttpMessageHandler<AuthenticatedHttpClientHandler>();

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
        options.Cookie.HttpOnly = true; // Más seguro: no accesible desde JavaScript
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Solo HTTPS
        options.ExpireTimeSpan = TimeSpan.FromHours(8); // Tiempo de expiración
        options.SlidingExpiration = true; // Renovación automática
    });

builder.Services.AddAuthorizationCore(options =>
{
    options.AddPolicy("SuperAdminPolicy", policy => policy.RequireRole("SuperAdmin"));
});

builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped(sp => (CustomAuthenticationStateProvider)sp.GetRequiredService<AuthenticationStateProvider>());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.MapDefaultEndpoints();

app.Run();
