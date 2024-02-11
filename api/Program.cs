using api.Helpers;
using api.Hubs;
using static api.Helpers.LogManager;
using Path = System.IO.Path;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

IConfiguration config = new ConfigurationBuilder()
    .AddIniFile(Path.Combine(Directory.GetCurrentDirectory(), "config.properties"), optional: false, reloadOnChange: false)
    .Build();

ConfigManager.SetConfig(config);
builder.Services.AddSingleton<LogManager>();
LogManager log = new(LogType.Info, "Server started");

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();
app.UseHttpsRedirection();
app.MapHub<ProgressHub>("/api/ws/progressHub");
app.MapFallbackToFile("/index.html");

app.Run();
