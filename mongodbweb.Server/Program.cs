using mongodbweb.Server.Helpers;
using static mongodbweb.Server.Helpers.LogManager;
using Path = System.IO.Path;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
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

// Configure the HTTP request pipeline.
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
