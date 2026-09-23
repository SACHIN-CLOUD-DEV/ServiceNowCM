using ServiceNowCM.Api.Middleware;
using ServiceNowCM.Application.Interfaces;
using ServiceNowCM.Application.Services;
using ServiceNowCM.ContentManager.Services;
using ServiceNowCM.Infrastructure;
using ServiceNowCM.Infrastructure.Repositories;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// ---------------------------------------------------------
// Data Protection
//
// Persist encryption keys outside the source-code folder so
// encrypted credentials remain readable even if the project
// is moved or freshly cloned.
// ---------------------------------------------------------

var dataProtectionKeysPath = Path.Combine(
    Environment.GetFolderPath(
        Environment.SpecialFolder.LocalApplicationData),
    "ServiceNowCM",
    "DataProtectionKeys");

Directory.CreateDirectory(dataProtectionKeysPath);

builder.Services
    .AddDataProtection()
    .SetApplicationName("ServiceNowCM")
    .PersistKeysToFileSystem(
        new DirectoryInfo(dataProtectionKeysPath));

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
