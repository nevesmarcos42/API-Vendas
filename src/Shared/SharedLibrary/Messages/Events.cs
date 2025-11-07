// Mensagens utilizadas na comunicação assíncrona entre microserviços
// Estas classes representam eventos que são publicados quando determinadas ações ocorrem

namespace SharedLibrary.Messages
{
    // Evento disparado quando uma venda é registrada
    // Utilizado pelo serviço de estoque para atualizar a quantidade disponível
    public class VendaRegistradaEvent
    {
        public int VendaId { get; set; }
        public int ProdutoId { get; set; }
        public int Quantidade { get; set; }
        public string Cliente { get; set; } = string.Empty;
        public DateTime DataVenda { get; set; }
        public decimal ValorTotal { get; set; }
    }

    // Evento para notificar alterações no estoque
    // Permite que outros serviços sejam informados sobre mudanças na disponibilidade
    public class EstoqueAlteradoEvent
    {
        public int ProdutoId { get; set; }
        public int QuantidadeAnterior { get; set; }
        public int QuantidadeAtual { get; set; }
        public string TipoMovimentacao { get; set; } = string.Empty;
        public DateTime DataAlteracao { get; set; }
    }

    // Evento disparado quando um produto é criado
    // Utilizado para sincronizar informações entre os serviços
    public class ProdutoCriadoEvent
    {
        public int ProdutoId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public decimal Preco { get; set; }
        public int QuantidadeEstoque { get; set; }
        public DateTime DataCriacao { get; set; }
    }

    // Comando para verificar disponibilidade de estoque
    // Utilizado antes de confirmar uma venda
    public class VerificarEstoqueCommand
    {
        public int ProdutoId { get; set; }
        public int QuantidadeSolicitada { get; set; }
        public string CorrelationId { get; set; } = string.Empty;
    }

    // Resposta para verificação de estoque
    public class EstoqueVerificadoResponse
    {
        public string CorrelationId { get; set; } = string.Empty;
        public bool DisponibilidadeSuficiente { get; set; }
        public int QuantidadeDisponivel { get; set; }
        public string Mensagem { get; set; } = string.Empty;
    }
}