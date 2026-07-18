using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PasteleriaNancys.Api.Middleware;
using PasteleriaNancys.Application.Administracion;
using PasteleriaNancys.Application.Caja;
using PasteleriaNancys.Application.Inventario;
using PasteleriaNancys.Application.Pedidos;
using PasteleriaNancys.Application.Reportes;
using PasteleriaNancys.Application.Seguridad;
using PasteleriaNancys.Application.Seguridad.Interfaces;
using PasteleriaNancys.Infrastructure.Caja;
using PasteleriaNancys.Infrastructure.Data;
using PasteleriaNancys.Infrastructure.Inventario;
using PasteleriaNancys.Infrastructure.Pedidos;
using PasteleriaNancys.Infrastructure.Seguridad;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Ingrese el token JWT: Bearer {token}"
    });
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSeguridadApplication();
builder.Services.AddSeguridadInfrastructure(builder.Configuration);

builder.Services.AddInventarioApplication();
builder.Services.AddInventarioInfrastructure();

builder.Services.AddCajaApplication();
builder.Services.AddCajaInfrastructure();

builder.Services.AddPedidosApplication();
builder.Services.AddPedidosInfrastructure();

builder.Services.AddReportesApplication();

builder.Services.AddAdministracionApplication();

QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

var jwtSection = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSection["Issuer"],
        ValidAudience = jwtSection["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!))
    };
    options.Events = new JwtBearerEvents
    {
        // El token solo prueba quién es el usuario al momento de emitirse; si lo desactivan o
        // bloquean después, el token seguiría siendo válido hasta expirar (hasta Jwt:ExpireMinutes)
        // si no volviéramos a chequear el estado real contra la BD en cada request.
        OnTokenValidated = async context =>
        {
            var idClaim = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (idClaim is null || !Guid.TryParse(idClaim, out var idUsuario))
            {
                context.Fail("Token inválido.");
                return;
            }

            var usuarioRepository = context.HttpContext.RequestServices.GetRequiredService<IUsuarioRepository>();
            var usuario = await usuarioRepository.ObtenerPorIdAsync(idUsuario);
            if (usuario is null || !usuario.Activo || usuario.Bloqueado)
            {
                context.Fail("La cuenta ya no está activa.");
            }
        }
    };
});

builder.Services.AddAuthorization();

const string FrontendCorsPolicy = "FrontendCorsPolicy";
var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:3000", "http://localhost:5173" };

builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
    {
        policy.WithOrigins(corsOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseCors(FrontendCorsPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
