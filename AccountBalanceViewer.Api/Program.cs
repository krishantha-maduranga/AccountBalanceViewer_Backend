using AccountBalanceViewer.Api.Middleware;
using AccountBalanceViewer.Application;
using AccountBalanceViewer.Persistence;
using AccountBalanceViewer.Persistence.Services;
using Microsoft.OpenApi.Models;
using Serilog;

Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateBootstrapLogger();

Log.Information("ABV API starting.");

var builder = WebApplication.CreateBuilder(args);

// Setup Serilog as the logging provider and look at the appsettings.json for configurations
builder.Host.UseSerilog((context, LoggerConfiguration) => LoggerConfiguration.WriteTo.Console()
                .ReadFrom.Configuration(context.Configuration));

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Add JWT Bearer Security Definition
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your valid token.\nExample: Bearer abc123"
    });

    // Add global security requirement (adds lock icon)
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Id = "Bearer",
                    Type = ReferenceType.SecurityScheme
                }
            },
            new List<string>()
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("ABVAPI", builder => builder.AllowAnyOrigin()
        .AllowAnyHeader().AllowAnyMethod());
});

builder.Services.AddApplicationServices();
builder.Services.AddPersistenceServices(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await IdentitySeeder.SeedUsersAndRolesAsync(services);
}

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

app.UseHttpsRedirection();

app.UseAuthentication();

// Custom Middleware for exception handling
app.UseCustomExceptionHandler();

app.UseCors("ABVAPI");

app.UseAuthorization();

app.MapControllers();

app.UseSerilogRequestLogging();

app.Run();
