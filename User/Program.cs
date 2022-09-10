using Common.Extensions;
using MongoDB.Entities;
using User.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IUserService, UserService>();


builder.Services.AddOpenTelemetryTracing(builder.Configuration);
builder.AddOpenTelemetryLoggin(builder.Configuration);
builder.AddOpenTelemetryMetrics(builder.Configuration);

// MongoDB
string DatabaseName = builder.Configuration.GetSection("Database")["DatabaseName"];
string DatabaseAddress = builder.Configuration.GetSection("Database")["DatabaseAddress"];
string DatabasePort = builder.Configuration.GetSection("Database")["DatabasePort"];
DB.InitAsync(DatabaseName, DatabaseAddress, int.Parse(DatabasePort));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
