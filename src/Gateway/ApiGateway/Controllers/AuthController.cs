// Controller responsável pela autenticação de usuários
// Gera tokens JWT que são utilizados para autorizar acesso aos microserviços

using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SharedLibrary.Constants;
using SharedLibrary.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ApiGateway.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IConfiguration configuration, ILogger<AuthController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        // Autentica usuário e retorna token JWT
        // Em um ambiente real, validaria contra banco de dados ou provider externo
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto loginDto)
        {
            try
            {
                // Validação simples para demonstração
                // Em produção, validar contra base de dados com hash de senha
                var user = ValidateUser(loginDto.Usuario, loginDto.Senha);
                
                if (user == null)
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        Sucesso = false,
                        Mensagem = "Credenciais inválidas",
                        Erros = new List<string> { "Usuário ou senha incorretos" }
                    });
                }

                var token = GenerateJwtToken(user);
                
                _logger.LogInformation("Login realizado com sucesso para o usuário: {Usuario}", loginDto.Usuario);
                
                return Ok(new ApiResponse<object>
                {
                    Sucesso = true,
                    Mensagem = "Login realizado com sucesso",
                    Dados = new { Token = token, Usuario = user.Nome, Role = user.Role }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante processo de login");
                return StatusCode(500, new ApiResponse<object>
                {
                    Sucesso = false,
                    Mensagem = "Erro interno do servidor",
                    Erros = new List<string> { "Falha no processo de autenticação" }
                });
            }
        }

        // Valida credenciais do usuário
        // Implementação simplificada para demonstração
        private UserInfo? ValidateUser(string username, string password)
        {
            // Em produção, consultar base de dados com senhas hasheadas
            var validUsers = new Dictionary<string, (string password, string role)>
            {
                { "admin", ("admin123", UserRoles.Admin) },
                { "vendedor", ("vend123", UserRoles.Vendedor) },
                { "estoquista", ("est123", UserRoles.Estoquista) }
            };

            if (validUsers.TryGetValue(username.ToLower(), out var userInfo) && 
                userInfo.password == password)
            {
                return new UserInfo
                {
                    Nome = username,
                    Role = userInfo.role
                };
            }

            return null;
        }

        // Gera token JWT com claims do usuário
        private string GenerateJwtToken(UserInfo user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"];
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];
            var expirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"] ?? "60");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Nome),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, 
                    new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), 
                    ClaimValueTypes.Integer64)
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // Classe auxiliar para informações do usuário
        private class UserInfo
        {
            public string Nome { get; set; } = string.Empty;
            public string Role { get; set; } = string.Empty;
        }
    }

    // DTO para receber dados de login
    public class LoginDto
    {
        public string Usuario { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
    }
}