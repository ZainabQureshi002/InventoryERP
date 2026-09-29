using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using InventoryERP.Web.Authorization;
using InventoryERP.Web.Data;
using InventoryERP.Web.Filters;
using InventoryERP.Web.Models;
using InventoryERP.Web.Services;
using InventoryERP.Web.Services.Ai;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, AppUserClaimsPrincipalFactory>();

builder.Services.AddAuthorization(options =>
{
    foreach (var permission in Permissions.All())
    {
        options.AddPolicy(permission, policy =>
            policy.RequireAssertion(ctx =>
                ctx.User.IsInRole("Admin") || ctx.User.HasClaim(Permissions.ClaimType, permission)));
    }
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IActivityLogService, ActivityLogService>();

builder.Services.AddAntiforgery(options => options.HeaderName = "X-CSRF-TOKEN");

builder.Services.AddScoped<AiDataToolsService>();
builder.Services.AddHttpClient<IAiAssistantService, AiAssistantService>(client =>
{
    var ollamaBaseUrl = builder.Configuration["Ollama:BaseUrl"] ?? "http://localhost:11434";
    client.BaseAddress = new Uri(ollamaBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(120);
});

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<ActivityLogFilter>();
});

var app = builder.Build();

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
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

using (var scope = app.Services.CreateScope())
{
    await DbSeeder.SeedAsync(scope.ServiceProvider);
    await PermissionSeeder.SeedAsync(scope.ServiceProvider);
}

app.Run();
