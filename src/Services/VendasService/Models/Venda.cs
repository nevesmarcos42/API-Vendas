// Entidades do domínio de vendas
// Representam as tabelas do banco de dados e regras de negócio relacionadas

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VendasService.Models
{
    // Entidade que representa uma venda no sistema
    // Contém todas as informações necessárias para rastrear uma transação
    [Table("Vendas")]
    public class Venda
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProdutoId { get; set; }

        [Required]
        [StringLength(200)]
        public string NomeProduto { get; set; } = string.Empty;

        [Required]
        public int Quantidade { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecoUnitario { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal ValorTotal { get; set; }

        [Required]
        [StringLength(100)]
        public string Cliente { get; set; } = string.Empty;

        [Required]
        public DateTime DataVenda { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = string.Empty;

        public DateTime DataCriacao { get; set; }
        public DateTime? DataAtualizacao { get; set; }

        // Método para calcular o valor total da venda
        public void CalcularValorTotal()
        {
            ValorTotal = Quantidade * PrecoUnitario;
        }

        // Método para validar se a venda está em um estado válido
        public bool IsValid()
        {
            return Quantidade > 0 && 
                   PrecoUnitario > 0 && 
                   !string.IsNullOrWhiteSpace(Cliente) &&
                   !string.IsNullOrWhiteSpace(NomeProduto);
        }
    }
}