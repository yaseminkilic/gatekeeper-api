using GatekeeperApi.Entities;
using GatekeeperApi.Infrastructure.Persistence;
using GatekeeperApi.Interfaces;
using GatekeeperApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

//builder.Services.AddOpenApi();
builder.Services.AddCors(o => o.AddPolicy("Angular", p =>
    p.WithOrigins("http://localhost:4200")
     .AllowAnyHeader()
     .AllowAnyMethod()));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        var jwt = builder.Configuration.GetSection("Jwt");
        var key = jwt["Key"]; // or jwt.GetValue<string>("Key"); if (string.IsNullOrWhiteSpace(key)) throw new InvalidOperationException("Missing configuration: Jwt:Key");

        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };

        // ... TokenValidationParameters aynı
        o.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = ctx =>
            {
                Console.WriteLine($"JWT HATA: {ctx.Exception.Message}");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(o =>
    o.AddPolicy("YonetimYetkisi", p => p.RequireRole("Admin")));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Yalnızca token'ı yapıştır, başına 'Bearer' yazma."
    });

    c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
    });
});

var app = builder.Build();

app.UseHttpsRedirection(); // app.UseHsts(); // production'da
app.UseMiddleware<SecurityHeadersMiddleware>();

app.UseCors("Angular");   // UseAuthorization'dan ÖNCE

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); // Generates the /openapi/v1.json file

    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.EnablePersistAuthorization();
    });

    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();

    await db.Database.MigrateAsync();

    if (!await db.Kullanicilar.AnyAsync())
    {
        var adminUser = new User { Ad = "admin", Rol = "Admin", KullaniciAdi = "admin@example.com" };
        var systemUser = new User { Ad = "system_user", Rol = "SystemUser", KullaniciAdi = "systemuser@example.com" };
        var standardUser = new User { Ad = "user", Rol = "User", KullaniciAdi = "user@example.com" };

        adminUser.SifreHash = hasher.HashPassword(adminUser, "Admin123!");
        systemUser.SifreHash = hasher.HashPassword(systemUser, "System123!");
        standardUser.SifreHash = hasher.HashPassword(standardUser, "User123!");

        db.Kullanicilar.AddRange(adminUser, systemUser, standardUser);
        await db.SaveChangesAsync();
    }
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

