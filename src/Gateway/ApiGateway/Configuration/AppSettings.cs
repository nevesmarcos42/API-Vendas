// Configurações específicas para autenticação e autorização
// Centraliza as configurações de JWT e políticas de segurança

namespace ApiGateway.Configuration
{
    // Configurações do JWT utilizadas pelo gateway
    public class JwtSettings
    {
        public string SecretKey { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int ExpirationMinutes { get; set; }
    }

    // Configurações de endpoints dos microserviços
    // Permite alterar URLs dos serviços sem recompilar
    public class ServiceEndpoints
    {
        public string VendasServiceUrl { get; set; } = string.Empty;
        public string EstoqueServiceUrl { get; set; } = string.Empty;
    }
}