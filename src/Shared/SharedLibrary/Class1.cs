// DTOs e interfaces compartilhadas entre os microserviços
// Esta biblioteca contém os contratos de dados e mensagens utilizados
// para comunicação entre os serviços de vendas, estoque e gateway

namespace SharedLibrary.DTOs
{
    // Representa os dados de um produto no sistema
    public class ProdutoDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public decimal Preco { get; set; }
        public int QuantidadeEstoque { get; set; }
        public string Categoria { get; set; } = string.Empty;
        public DateTime DataCriacao { get; set; }
    }

    // Dados necessários para criar um novo produto
    public class CriarProdutoDto
    {
        public string Nome { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public decimal Preco { get; set; }
        public int QuantidadeEstoque { get; set; }
        public string Categoria { get; set; } = string.Empty;
    }

    // Representa uma venda no sistema
    public class VendaDto
    {
        public int Id { get; set; }
        public int ProdutoId { get; set; }
        public string NomeProduto { get; set; } = string.Empty;
        public int Quantidade { get; set; }
        public decimal PrecoUnitario { get; set; }
        public decimal ValorTotal { get; set; }
        public string Cliente { get; set; } = string.Empty;
        public DateTime DataVenda { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    // Dados para registrar uma nova venda
    public class CriarVendaDto
    {
        public int ProdutoId { get; set; }
        public int Quantidade { get; set; }
        public string Cliente { get; set; } = string.Empty;
    }

    // Representa movimentação de estoque
    public class MovimentacaoEstoqueDto
    {
        public int Id { get; set; }
        public int ProdutoId { get; set; }
        public string TipoMovimentacao { get; set; } = string.Empty; // Entrada, Saida, Ajuste
        public int Quantidade { get; set; }
        public string Motivo { get; set; } = string.Empty;
        public DateTime DataMovimentacao { get; set; }
    }

    // Dados para criar movimentação de estoque
    public class CriarMovimentacaoEstoqueDto
    {
        public int ProdutoId { get; set; }
        public string TipoMovimentacao { get; set; } = string.Empty;
        public int Quantidade { get; set; }
        public string Motivo { get; set; } = string.Empty;
    }

    // Resposta padrão da API
    public class ApiResponse<T>
    {
        public bool Sucesso { get; set; }
        public string Mensagem { get; set; } = string.Empty;
        public T? Dados { get; set; }
        public List<string> Erros { get; set; } = new();
    }
}
