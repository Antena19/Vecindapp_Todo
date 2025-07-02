using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Microsoft.AspNetCore.Cors;
using REST_VECINDAPP.Servicios;
using REST_VECINDAPP.CapaNegocios;
using REST_VECINDAPP.Servicios.Archivos;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container - ANTES de builder.Build()
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors(options =>
{
    // Configuración CORS permisiva para permitir todas las conexiones
    options.AddDefaultPolicy(builder =>
    {
        builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Configurar Swagger para soportar JWT
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "VecindApp API",
        Version = "v1",
        Description = "API para la aplicacion de gestion de juntas de vecinos"
    });

    // Configurar Swagger para usar JWT
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando el esquema Bearer. Ingresa tu token JWT aqu�.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<cn_Usuarios>();
builder.Services.AddScoped<cn_Directiva>();
builder.Services.AddScoped<cn_Certificados>();
builder.Services.AddScoped<cn_SolicitudesCertificado>();
builder.Services.AddScoped<cn_MercadoPago>();
builder.Services.AddScoped<cn_Eventos>();
builder.Services.AddScoped<cn_Comunicacion>();
builder.Services.AddScoped<TransbankServiceV2>();
builder.Services.AddScoped<WebpayService>();
builder.Services.AddScoped<REST_VECINDAPP.Servicios.TransbankService>();

// Configurar la autenticacin con JWT
var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]);


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<REST_VECINDAPP.Seguridad.VerificadorRoles>();

// Configurar el puerto dinámico para Railway
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
{
    builder.WebHost.ConfigureKestrel(serverOptions =>
    {
        serverOptions.ListenAnyIP(int.Parse(port));
    });
}
// Si no hay variable PORT, usará los puertos del launchSettings.json

// Agregar el servicio de almacenamiento de archivos
builder.Services.AddScoped<FileStorageService>();
// Configurar el tama�o m�ximo de archivos (opcional)
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10MB
});





// Construir la aplicaci�n
var app = builder.Build();

// Configure the HTTP request pipeline - DESPU�S de builder.Build()
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "VecindApp API v1");
        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
        c.DefaultModelsExpandDepth(-1); // Oculta la secci�n de modelos por defecto
    });
    app.UseDeveloperExceptionPage();
}
app.UseCors(); // Usar la política por defecto (permisiva)
// app.UseHttpsRedirection(); // Desactivado para evitar redirección a HTTPS en local y Railway
app.UseAuthentication();  // Primero autenticacin
app.UseAuthorization();   // Luego autorizaci�n
app.MapControllers();

// Agregar soporte para archivos est�ticos
app.UseStaticFiles();

// Agregar endpoint de healthcheck
app.MapGet("/", () => Results.Ok("API is healthy"));

// Redirigir POST /payment/return a GET /payment/return para soporte de Webpay
app.MapPost("/payment/return", async context => {
    context.Response.Redirect("/payment/return");
});

// Endpoint de confirmación final para Webpay
app.MapGet("/payment/final", (HttpRequest request) =>
{
    return Results.Content(@"
<!DOCTYPE html>
<html lang='es'>
<head>
    <meta charset='UTF-8'>
    <title>Pago procesado</title>
    <style>
        body { font-family: 'Segoe UI', Arial, sans-serif; background: #fff; color: #222; margin: 0; padding: 0; }
        .container { max-width: 420px; margin: 60px auto; padding: 32px 24px; border-radius: 12px; border: 1px solid #e0e0e0; box-shadow: 0 2px 12px rgba(0,185,148,0.08); text-align: center; }
        .titulo { font-size: 1.7rem; font-weight: 700; color: #00897B; margin-bottom: 18px; }
        .mensaje { font-size: 1.1rem; margin-bottom: 12px; }
        .instruccion { font-size: 1rem; color: #555; }
    </style>
</head>
<body>
    <div class='container'>
        <div class='titulo'>Pago procesado</div>
        <div class='mensaje'>Tu pago fue realizado con éxito.</div>
        <div class='instruccion'>Puedes volver a la aplicación para descargar tu certificado.</div>
    </div>
</body>
</html>
", "text/html");
});



app.Run();