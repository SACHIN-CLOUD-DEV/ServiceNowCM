using ServiceNowCM.Api.Middleware;
using ServiceNowCM.Application.Interfaces;
using ServiceNowCM.Application.Services;
using ServiceNowCM.ContentManager.Services;
using ServiceNowCM.Infrastructure;
using ServiceNowCM.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<ServiceNowConnectionService>();

builder.Services.AddScoped<IntegrationService>();

builder.Services.AddScoped<IntegrationSyncService>();

builder.Services.AddScoped<ContentManagerConnectionService>();

builder.Services.AddScoped<IContentManagerClient, ContentManagerClient>();

builder.Services.AddScoped<ISyncedRecordRepository,SyncedRecordRepository>();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

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
