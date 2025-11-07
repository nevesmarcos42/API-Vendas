// Configuração principal do API Gateway
// Centraliza autenticação, autorização e roteamento para os microserviços

using ApiGateway.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SharedLibrary.Constants;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configuração de serviços do container DI
builder.Services.AddControllers();
// Verificações de saúde
builder.Services.AddHealthChecks();

// Configuração do Swagger com suporte a JWT
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "API Gateway - Sistema de Vendas", 
        Version = "v1",
        Description = "Gateway centralizado para microserviços de vendas e estoque"
    });
    
    // Configuração para autenticação JWT no Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Autorização JWT usando o esquema Bearer. Digite 'Bearer' [espaço] e o token na caixa de texto abaixo.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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

// Configuração da autenticação JWT
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!)),
            ClockSkew = TimeSpan.Zero
        };
    });

// Configuração de políticas de autorização baseadas em roles
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole(UserRoles.Admin));
    options.AddPolicy("VendedorOuAdmin", policy => 
        policy.RequireRole(UserRoles.Vendedor, UserRoles.Admin));
    options.AddPolicy("EstoquistaOuAdmin", policy => 
        policy.RequireRole(UserRoles.Estoquista, UserRoles.Admin));
});

// Configuração do HttpClient para comunicação com microserviços
builder.Services.AddHttpClient();

// Configuração de CORS para permitir acesso de frontends
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Pipeline de requisições HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Gateway v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.UseCors();

// Middleware de autenticação deve vir antes da autorização
app.UseAuthentication();
app.UseAuthorization();

// Middleware customizado para proxy dos microserviços
// Deve vir depois da autenticação/autorização para ter acesso ao contexto do usuário
app.UseMiddleware<ServiceProxyMiddleware>();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
