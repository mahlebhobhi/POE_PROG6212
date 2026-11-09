using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Security;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<RaceDayDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("RaceDay")));
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".RaceDay.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<PasswordService>();
builder.Services.AddScoped<CurrentUser>();
builder.Services.AddSingleton<RaceDay.Api.Services.BlobStorageService>();

var app = builder.Build();
if (builder.Configuration.GetValue<bool>("Database:EnsureCreated"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<RaceDayDbContext>();
    var attempts = 0;
    while (true) { try { db.Database.EnsureCreated(); break; } catch when (++attempts < 20) { await Task.Delay(TimeSpan.FromSeconds(3)); } }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseSession();
app.MapControllers();
app.MapGet("/", () => Results.Ok(new { application = "RaceDay API", docs = "/swagger", status = "running" }));

app.Run();

public partial class Program { }
