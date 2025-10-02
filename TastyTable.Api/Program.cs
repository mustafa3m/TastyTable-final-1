using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using TastyTable.Core.Interfaces;
using TastyTable.Data;
using TastyTable.Services;
using TastyTable.Api.Swagger;

var builder = WebApplication.CreateBuilder(args);

// Controllers + JSON options
builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "TastyTable API", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Enter JWT with **Bearer** prefix (e.g., `Bearer eyJhbGci...`)",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.OperationFilter<AuthorizeCheckOperationFilter>();
    options.SchemaFilter<EmptyStringSchemaFilter>();
});

// EF Core + MySQL
var connStr = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connStr, ServerVersion.AutoDetect(connStr)));

// Services
builder.Services.AddSingleton<JwtTokenService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IMenuService, MenuService>();
builder.Services.AddScoped<IOrderService, OrderService>();

// JWT auth
var secret = builder.Configuration["Jwt:Secret"] ?? "ChangeThisSecretKey!";
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

builder.Services.AddAuthentication(o =>
{
    o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(o =>
{
    o.RequireHttpsMetadata = false;
    o.SaveToken = true;
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = key,
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero,
        RoleClaimType = System.Security.Claims.ClaimTypes.Role,
        NameClaimType = System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

// Run DB migrations + seed safely
using (var scope = app.Services.CreateScope())
{
    try
    {
        var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        ctx.Database.Migrate();
        DemoDataSeeder.Seed(ctx);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Startup] DB migration/seed failed: {ex.Message}");
    }
}

// Always enable Swagger (also in Production for EB)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "TastyTable API v1");
    c.DocumentTitle = "TastyTable API";
});

// Health & root endpoints
app.MapGet("/", () => Results.Redirect("/swagger"));
app.MapGet("/healthz", () => Results.Text("OK"));

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

// --- Seeder ---
public static class DemoDataSeeder
{
    public static void Seed(AppDbContext ctx)
    {
        if (!ctx.MenuItems.Any())
        {
            ctx.MenuItems.AddRange(
                new TastyTable.Core.Entities.MenuItem { Name = "Margherita Pizza", Description = "Classic pizza", Price = 1200, IsAvailable = true },
                new TastyTable.Core.Entities.MenuItem { Name = "Chicken Biryani", Description = "Spicy and flavorful", Price = 900, IsAvailable = true },
                new TastyTable.Core.Entities.MenuItem { Name = "Beef Burger", Description = "Grilled patty", Price = 750, IsAvailable = true }
            );
        }
        if (!ctx.Users.Any())
        {
            ctx.Users.Add(new TastyTable.Core.Entities.User
            {
                Username = "admin",
                Email = "admin@tastytable.local",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                Role = "Admin"
            });
        }
        ctx.SaveChanges();
    }
}
