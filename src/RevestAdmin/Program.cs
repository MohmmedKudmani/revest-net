using System.Globalization;
using MudBlazor;
using MudBlazor.Services;
using RevestAdmin.Components;
using RevestAdmin.Services;

var builder = WebApplication.CreateBuilder(args);

// Culture: ensure $1,299.00 and "Jan 5, 2025" formatting
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.GetCultureInfo("en-US");
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo("en-US");

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;
    config.SnackbarConfiguration.ShowCloseIcon = true;
    config.SnackbarConfiguration.VisibleStateDuration = 4000;
});

// HttpClients
builder.Services.AddHttpClient("ProductClient", c =>
    c.BaseAddress = new Uri(builder.Configuration["PRODUCT_SERVICE_URL"] ?? "http://localhost:3001/"));
builder.Services.AddHttpClient("OrderClient", c =>
    c.BaseAddress = new Uri(builder.Configuration["ORDER_SERVICE_URL"] ?? "http://localhost:3002/"));

// Services
builder.Services.AddScoped<ProductApiService>();
builder.Services.AddScoped<OrderApiService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

// No HTTPS redirect — backend services are HTTP
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
