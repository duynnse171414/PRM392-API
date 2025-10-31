using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MyApp.Business.Services;
using MyApp.Data;

var builder = WebApplication.CreateBuilder(args);

// Bind theo biến môi trường PORT nếu Render cung cấp
var renderPort = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(renderPort))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{renderPort}");
}

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ✅✅✅ THÊM CORS - Cho phép truy cập từ mọi nguồn
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()      // Cho phép mọi domain
              .AllowAnyMethod()      // Cho phép mọi HTTP method (GET, POST, PUT, DELETE...)
              .AllowAnyHeader();     // Cho phép mọi header
    });
});

// ✅ Đăng ký DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 21))
    ));


builder.Services.AddHttpClient();

// ✅ Đăng ký Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IModel3DService, Model3DService>();
builder.Services.AddScoped<IMembershipPackageService, MembershipPackageService>();
builder.Services.AddScoped<IGenerationHistoryService, GenerationHistoryService>();
builder.Services.AddScoped<IBackblazeStorageService, BackblazeStorageService>();
builder.Services.AddScoped<IVnPayService, VnPayService>();

builder.Services.AddControllers()
    .AddJsonOptions(x =>
    {
        x.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
        x.JsonSerializerOptions.WriteIndented = true;
    });

// ✅ Cấu hình Swagger với JWT
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Nhập JWT token theo format: Bearer {token}"
    });
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// ✅ Cấu hình JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!))
    };
});

// ✅ Thêm Authorization
builder.Services.AddAuthorization();

var app = builder.Build();

// ✅✅✅ TỰ ĐỘNG MIGRATION DATABASE KHI DEPLOY
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        var logger = services.GetRequiredService<ILogger<Program>>();
        
        logger.LogInformation("🔄 Đang kiểm tra và migration database...");
        
        // ✅ SỬ DỤNG MIGRATE() thay vì EnsureCreated()
        context.Database.Migrate();
        
        logger.LogInformation("✅ Database migration hoàn tất!");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "❌ Lỗi khi migration database: {Message}", ex.Message);
        
        // Có thể throw lại nếu muốn app dừng khi migration fail
        // throw;
    }
}
// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (Environment.GetEnvironmentVariable("ENABLE_SWAGGER") == "true")
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

// ✅ QUAN TRỌNG: Thứ tự phải đúng
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
