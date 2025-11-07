// Constantes utilizadas em toda a aplicação
// Centraliza valores que são compartilhados entre os microserviços

namespace SharedLibrary.Constants
{
    // Nomes das filas do RabbitMQ
    // Padronização para evitar erros de digitação e facilitar manutenção
    public static class QueueNames
    {
        public const string VendaRegistrada = "venda.registrada";
        public const string EstoqueAlterado = "estoque.alterado";
        public const string ProdutoCriado = "produto.criado";
        public const string VerificarEstoque = "estoque.verificar";
        public const string EstoqueVerificado = "estoque.verificado";
    }

    // Status possíveis para vendas
    public static class StatusVenda
    {
        public const string Pendente = "Pendente";
        public const string Confirmada = "Confirmada";
        public const string Cancelada = "Cancelada";
        public const string Entregue = "Entregue";
    }

    // Tipos de movimentação de estoque
    public static class TipoMovimentacao
    {
        public const string Entrada = "Entrada";
        public const string Saida = "Saida";
        public const string Ajuste = "Ajuste";
        public const string Venda = "Venda";
    }

    // Configurações de JWT
    public static class JwtConstants
    {
        public const string Issuer = "MicroservicesVendas";
        public const string Audience = "MicroservicesVendas.Client";
        public const int ExpirationMinutes = 60;
    }

    // Roles de usuário
    public static class UserRoles
    {
        public const string Admin = "Admin";
        public const string Vendedor = "Vendedor";
        public const string Estoquista = "Estoquista";
    }
}