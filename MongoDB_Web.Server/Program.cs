using MongoDB_Web.Server.Filters;
using MongoDB_Web.Server.Helpers;
using static MongoDB_Web.Server.Helpers.LogManager;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

IConfiguration config = new ConfigurationBuilder()
    .AddIniFile(Path.Combine(Directory.GetCurrentDirectory(), "config.properties"), optional: false, reloadOnChange: false)
    .Build();

ConfigManager configManager = new(config);
builder.Services.AddSingleton<ConfigManager>(configManager);
builder.Services.AddSingleton<LogManager>();
builder.Services.AddSingleton<OTPFileManagement>();
LogManager log = new(LogType.Info, "Server started");

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();
