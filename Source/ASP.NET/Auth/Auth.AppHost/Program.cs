using Auth.AppHost.Configuration;
using Auth.RepositoryImplement.Context;
using Auth.AppHost.DependencyInjection;
using Common.Web.HealthCheck;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<CoreDBContext>();
builder.Services.AddAuthConfigs(builder.Configuration);
builder.Services.AddDependencyInjection(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddOpenApi(); // Learn more at https://aka.ms/aspnet/openapi

var app = builder.Build();

// Configure the HTTP request pipeline.

await app.EnsureDatabaseHealthy<CoreDBContext>();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

if (app.Environment.IsDevelopment())
{
  app.MapOpenApi();
}

app.Run();
