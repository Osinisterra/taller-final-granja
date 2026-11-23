using System.Reflection;
using System.Text.Json.Serialization;
using Granja.Infrastructure;
using Granja.Infrastructure.Data;
using Granja.Services;
using Granja.Web.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Raíz de composición: Web registra Services e Infrastructure (los controllers solo ven interfaces de Services).
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Manejo global de errores con ProblemDetails.
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Granja lechera API", Version = "v1", Description = "Producción de leche, inventario FIFO por lotes, ventas y reportes." });
    var xml = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
    if (File.Exists(xml)) c.IncludeXmlComments(xml);
});

builder.Services.AddCors(p => p.AddPolicy("CORS_Policy", b => b.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

await DbInitializer.InicializarAsync(app.Services);

app.UseExceptionHandler();
app.UseStatusCodePages();

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Granja lechera API v1"));

app.UseCors("CORS_Policy");
app.MapControllers();
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

app.Run();

public partial class Program;
