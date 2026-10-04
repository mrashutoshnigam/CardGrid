using CardGrid.Core.Services;
using CardGrid.Database;
using Microsoft.AspNetCore.SystemWebAdapters;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpForwarder();
builder.Services.AddSystemWebAdapters();
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
builder.Services.AddProblemDetails();
builder.Services
    .AddControllersWithViews()
    // CardGrid.js reads the payload with the original member names (Data, total, noOfPages, ...).
    .AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null);

builder.Services.AddOptions<DatabaseOptions>().BindConfiguration(DatabaseOptions.SectionName);
builder.Services.AddScoped(sp => new CardGridContext(
    sp.GetRequiredService<IConfiguration>().GetConnectionString("DatabaseModelContainer")
        ?? throw new InvalidOperationException("Connection string 'DatabaseModelContainer' is not configured.")));
builder.Services.AddScoped<IEmployeeStore>(sp => sp.GetRequiredService<CardGridContext>());
builder.Services.AddScoped<EmployeeGridService>();

var app = builder.Build();

app.InitializeCardGridDatabase();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler();
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSystemWebAdapters();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Default}/{action=Index}/{id?}");

// Side-by-side migration: anything this host does not handle falls through to the legacy app.
var proxyTo = app.Configuration["ProxyTo"];
if (!string.IsNullOrWhiteSpace(proxyTo))
{
    app.MapForwarder("/{**catch-all}", proxyTo);
}

app.Run();
