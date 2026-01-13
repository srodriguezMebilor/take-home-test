using System;
using System.Threading; // Necesario para el Sleep
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Serilog;
using Fundo.Applications.WebApi.Data;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// 1. Logs
var seqUrl = builder.Configuration["Serilog:SeqServerUrl"] ?? "http://seq:5341";
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.Seq(seqUrl)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

// ---------------------------------------------------------------------------
// 2. Base de Datos (MODIFICADO PARA EXTENDER TIEMPO DE ESPERA)
// ---------------------------------------------------------------------------
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions => 
    {
        // CAMBIO CRÍTICO: Aumentamos el tiempo de espera a 180 segundos (3 minutos).
        // Por defecto son 30s, lo cual es muy poco para crear la DB en Docker la primera vez.
        sqlOptions.CommandTimeout(180);

        // Habilitamos reintentos automáticos para micro-cortes de red
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 10, 
            maxRetryDelay: TimeSpan.FromSeconds(5), 
            errorNumbersToAdd: null);
    }));

builder.Services.AddScoped<IAppDbContext>(provider =>
    provider.GetRequiredService<AppDbContext>());

// 3. CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 4. Servicios API
/*
builder.Services.AddControllers().ConfigureApiBehaviorOptions(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        // Tomamos el primer error encontrado
        var error = context.ModelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage)
            .FirstOrDefault();

        return new BadRequestObjectResult(new { message = error });
    };
});*/

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 5. Middleware
app.UseSwagger();
app.UseSwaggerUI();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
}

app.UseCors("AllowAngular");
app.UseAuthorization();
app.MapControllers();

// ---------------------------------------------------------------------------
// 6. INICIALIZACIÓN ROBUSTA (ESPERA A SQL SERVER)
// ---------------------------------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var maxRetries = 10;
    var retryDelay = TimeSpan.FromSeconds(3);
    var currentRetry = 0;
    var dbInitialized = false;

    while (currentRetry < maxRetries && !dbInitialized)
    {
        try
        {
            var db = services.GetRequiredService<AppDbContext>();
            
            // Intenta crear la DB y ejecutar el Seeding (Insertar datos).
            // Gracias al CommandTimeout(180) de arriba, ahora esperará pacientemente.
            db.Database.EnsureCreated();
            
            Log.Information("✅ Base de datos conectada y verificada exitosamente.");
            dbInitialized = true; // Salimos del bucle
        }
        catch (Exception ex)
        {
            currentRetry++;
            Log.Warning(ex, $"⏳ SQL Server no está listo o está ocupado creando archivos. Intento {currentRetry} de {maxRetries}. Esperando {retryDelay.TotalSeconds} segundos...");
            
            // Esperamos antes de reintentar
            Thread.Sleep(retryDelay);
        }
    }

    if (!dbInitialized)
    {
        Log.Error("❌ No se pudo conectar a SQL Server después de múltiples intentos. La aplicación podría no funcionar correctamente.");
    }
}
// ---------------------------------------------------------------------------

app.Run();