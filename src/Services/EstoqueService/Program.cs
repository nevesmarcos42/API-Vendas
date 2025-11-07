using EstoqueService.Data;
using EstoqueService.Repositories;
using EstoqueService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Adiciona serviços ao container DI
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Banco de dados
builder.Services.AddDbContext<EstoqueDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositórios
builder.Services.AddScoped<IProdutoRepository, ProdutoRepository>();

// Serviços
builder.Services.AddScoped<IProdutoService, ProdutoService>();

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

// Verificações de saúde
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configuração do pipeline de requisições HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

// Criação do banco de dados
// Cria/migra banco de dados de forma segura (tenta Migrate primeiro, depois EnsureCreated)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetService<ILogger<Program>>();
    try
    {
        var context = services.GetRequiredService<EstoqueDbContext>();
        // Tenta aplicar migrações (preferível). Se não há migrações, usa EnsureCreated.
        try
        {
            context.Database.Migrate();
            logger?.LogInformation("Database migrated for EstoqueService.");
        }
        catch (Exception migrateEx)
        {
            logger?.LogWarning(migrateEx, "Migrate failed, attempting EnsureCreated as fallback.");
            try
            {
                context.Database.EnsureCreated();
                logger?.LogInformation("Database ensured (created) for EstoqueService.");
            }
            catch (Exception ensureEx)
            {
                logger?.LogError(ensureEx, "EnsureCreated also failed for EstoqueService.");
                throw; // relança para que o container registre o erro e possa ser inspecionado
            }
        }
    }
    catch (Exception ex)
    {
        // Se algo der errado durante a inicialização do BD, registra e relança para tornar a falha visível
        var loggerFallback = scope.ServiceProvider.GetService<ILogger<Program>>();
        loggerFallback?.LogError(ex, "Unhandled exception while initializing EstoqueService database.");
        throw;
    }
}

app.Run();
