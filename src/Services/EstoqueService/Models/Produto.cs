// Entidades do domínio de estoque e produtos
// Representam as tabelas do banco de dados e regras de negócio relacionadas

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EstoqueService.Models
{
    // Entidade que representa um produto no sistema
    // Contém informações básicas do produto e seu estoque atual
    [Table("Produtos")]
    public class Produto
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Nome { get; set; } = string.Empty;

        [StringLength(500)]
        public string Descricao { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Preco { get; set; }

        [Required]
        public int QuantidadeEstoque { get; set; }

        [Required]
        [StringLength(100)]
        public string Categoria { get; set; } = string.Empty;

        public DateTime DataCriacao { get; set; }
        public DateTime? DataAtualizacao { get; set; }

        // Coleção de movimentações de estoque relacionadas ao produto
        public virtual ICollection<MovimentacaoEstoque> Movimentacoes { get; set; } = new List<MovimentacaoEstoque>();

        // Método para verificar se há estoque suficiente
        public bool TemEstoqueSuficiente(int quantidadeSolicitada)
        {
            return QuantidadeEstoque >= quantidadeSolicitada;
        }

        // Método para atualizar estoque
        public void AtualizarEstoque(int quantidade, string tipoMovimentacao)
        {
            switch (tipoMovimentacao.ToLower())
            {
                case "entrada":
                case "ajuste":
                    QuantidadeEstoque += quantidade;
                    break;
                case "saida":
                case "venda":
                    QuantidadeEstoque -= quantidade;
                    if (QuantidadeEstoque < 0)
                        QuantidadeEstoque = 0;
                    break;
            }
        }

        // Método para validar se o produto está em um estado válido
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Nome) && 
                   Preco >= 0 && 
                   QuantidadeEstoque >= 0 &&
                   !string.IsNullOrWhiteSpace(Categoria);
        }
    }

    // Entidade que representa movimentações de estoque
    // Registra todas as entradas e saídas de produtos
    [Table("MovimentacoesEstoque")]
    public class MovimentacaoEstoque
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProdutoId { get; set; }

        [Required]
        [StringLength(50)]
        public string TipoMovimentacao { get; set; } = string.Empty; // Entrada, Saida, Ajuste, Venda

        [Required]
        public int Quantidade { get; set; }

        public int QuantidadeAnterior { get; set; }
        public int QuantidadeAtual { get; set; }

        [StringLength(300)]
        public string Motivo { get; set; } = string.Empty;

        [Required]
        public DateTime DataMovimentacao { get; set; }

        public DateTime DataCriacao { get; set; }

        // Navegação para o produto relacionado
        public virtual Produto Produto { get; set; } = null!;

        // Método para validar se a movimentação é válida
        public bool IsValid()
        {
            return ProdutoId > 0 && 
                   Quantidade > 0 && 
                   !string.IsNullOrWhiteSpace(TipoMovimentacao);
        }
    }
}