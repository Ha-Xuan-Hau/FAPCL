using System.Net;
using FAPCLClient.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DBContextConnection") ?? throw new InvalidOperationException("Connection string 'DBContextConnection' not found.");

builder.Services.AddDbContext<BookClassRoomContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<BookClassRoomContext>();

// Add services to the container.
builder.Services.AddRazorPages();

// Cấu hình HttpClient để gọi API từ MyApi
builder.Services.AddHttpClient("FAPCL", client =>
{
    client.BaseAddress = new Uri("https://localhost:5001/api/");
});
builder.Services.AddSession();
builder.Services.AddSignalR();
builder.WebHost.ConfigureKestrel(options =>
{
    options.Listen(IPAddress.Any, 7005); // Đảm bảo ứng dụng lắng nghe trên cổng 7005
});

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

app.UseAuthorization();

app.MapRazorPages();
app.UseSession();

app.Run();
