using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Nearest.Data;
using Nearest.Mappings;
using Nearest.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// AutoMapper
builder.Services.AddAutoMapper(typeof(CompanyMappingProfile), typeof(TicketMappingProfile), typeof(AddressMappingProfile), typeof(AdminMappingProfile), typeof(TowTruckMappingProfile));

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"] ?? "your-super-secret-key-that-is-at-least-32-characters-long")),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// Services
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<ITicketService, TicketService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ITowTruckService, TowTruckService>();

// Address Services
builder.Services.AddScoped<IAddressService, AddressService>();
builder.Services.AddScoped<IAddressHelperService, AddressHelperService>();

// Repositories
builder.Services.AddScoped<Nearest.Repositories.ICityRepository, Nearest.Repositories.CityRepository>();
builder.Services.AddScoped<Nearest.Repositories.IDistrictRepository, Nearest.Repositories.DistrictRepository>();
builder.Services.AddScoped<Nearest.Repositories.ICityDistrictRepository, Nearest.Repositories.CityDistrictRepository>();
builder.Services.AddScoped<Nearest.Repositories.IAdminRepository, Nearest.Repositories.AdminRepository>();

// HttpClient for external API calls
builder.Services.AddHttpClient();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Nearest API", Version = "v1" });
    
    // JWT Authentication için Swagger yapılandırması
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

// Static files for uploads
app.UseStaticFiles();

// Ensure upload directories exist at startup
var webRoot = app.Environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
Directory.CreateDirectory(Path.Combine(webRoot, "uploads", "drivers"));

app.MapControllers();

// Database migration and default admin creation
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate();

    // Create default admin if not exists
    var adminService = scope.ServiceProvider.GetRequiredService<IAdminService>();
    await adminService.CreateDefaultAdminAsync();
}

app.Run();
