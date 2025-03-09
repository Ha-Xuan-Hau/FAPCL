using FAPCL.Model;
using FAPCL.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity.UI.Services;
using Ebay_Project_PRN.Helper;

var builder = WebApplication.CreateBuilder(args);
var jwtSettings = builder.Configuration.GetSection("Jwt");

// Các dịch vụ cho controllers, Swagger và Kestrel
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//builder.WebHost.ConfigureKestrel(options =>{
 //   options.ListenAnyIP(5000);
  //  options.ListenAnyIP(5001, listenOptions => listenOptions.UseHttps());});

// Cấu hình DbContext và Identity
builder.Services.AddDbContext<BookClassRoomContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Cấu hình Identity với đầy đủ các dịch vụ cho AspNetUser và AspNetRole
builder.Services.AddIdentity<AspNetUser, IdentityRole>()
    .AddEntityFrameworkStores<BookClassRoomContext>()
    .AddDefaultTokenProviders(); // Đảm bảo có DefaultTokenProviders cho các tính năng như xác thực mật khẩu, xác minh email, v.v.

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddTransient<IEmailSender, EmailSender>();

// Đăng ký thêm RoleManager và UserManager cho Identity
builder.Services.AddScoped<UserManager<AspNetUser>>();
builder.Services.AddScoped<RoleManager<IdentityRole>>();

// Cấu hình Authentication với Bearer Token
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"], // Lấy từ cấu hình
            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"], // Lấy từ cấu hình
            ValidateLifetime = true,
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtSettings["Key"])) // Lấy từ cấu hình
        };
    });

// Cấu hình Authorization
builder.Services.AddAuthorization();

// Đảm bảo rằng Swagger chỉ có thể truy cập khi có quyền truy cập hợp lệ
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Please enter token"
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
            new string[] { }
        }
    });
});

builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IRoomService, RoomService>();

builder.Services.AddControllers();

var app = builder.Build();

// Phần cấu hình cho môi trường phát triển và Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Middleware xử lý HTTPS và Authentication
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
