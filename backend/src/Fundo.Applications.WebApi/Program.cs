using Fundo.Applications.WebApi.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog; 

var builder = WebApplication.CreateBuilder(args);

// Obtener la cadena de conexión
// Nota: Docker sobreescribirá esto automáticamente a través de variables de entorno
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");


// Registrar el DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// Inyección de dependencia. Registramos la interfaz para que el contenedor use la misma instancia (Scoped)
builder.Services.AddScoped<IAppDbContext>(provider =>
    provider.GetRequiredService<AppDbContext>());



// Configuración de Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.Seq("http://localhost:5341") // Dirección de Seq en tu PC
    .Enrich.FromLogContext()
    .CreateLogger();
// 2. Decirle al Builder que use Serilog
builder.Host.UseSerilog();


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Aplicar migraciones automáticamente al iniciar (de utilidad en Docker)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<IAppDbContext>();
    db.Migrate();
    
}

app.UseAuthorization();
app.MapControllers();
app.Run();