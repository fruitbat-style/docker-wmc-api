using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WMCApi;
using WMCApi.Data;
using WMCApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonConfig.SnakeCaseOptions.PropertyNamingPolicy;
    });
builder.Services.AddOpenApi();

builder.Services.AddDbContext<WmcDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<WmcDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.Name = "WmcAdmin";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = 401;
        return Task.CompletedTask;
    };
});

builder.Services.AddScoped<ILocationService, LocationService>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
        else
        {
            var origins = builder.Configuration["AllowedOrigins"]?.Split(',') ?? [];
            policy.WithOrigins(origins)
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Run migrations and seed data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<WmcDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    await db.Database.MigrateAsync();
    await SeedData.SeedLocationsAsync(db, app.Environment.ContentRootPath, logger);

    // Seed default admin user
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    if (await userManager.FindByNameAsync("admin") is null)
    {
        var admin = new IdentityUser { UserName = "admin" };
        await userManager.CreateAsync(admin, "Admin123!");
        logger.LogInformation("Seeded default admin user (admin / Admin123!)");
    }
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
