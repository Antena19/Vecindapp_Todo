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
    options.AddPolicy("AllowSpecificOrigin",
        builder => builder
            .WithOrigins("http://localhost:8100", "https://tudominio.com",
                "capacitor://localhost") // URL de tu app Ionic
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
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
builder.Services.AddScoped<cn_MercadoPago>();
builder.Services.AddScoped<cn_Eventos>();

// Configurar la autenticacin con JWT
var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key no est� configurado"));

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
}
app.UseCors("AllowSpecificOrigin");
app.UseHttpsRedirection();
app.UseAuthentication();  // Primero autenticaci�n
app.UseAuthorization();   // Luego autorizaci�n
app.MapControllers();

// Agregar soporte para archivos est�ticos
app.UseStaticFiles();

app.Run();