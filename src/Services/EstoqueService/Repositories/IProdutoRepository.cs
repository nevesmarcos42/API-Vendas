// Interface para operações de repositório de produtos
// Define o contrato para acesso aos dados de produtos

using EstoqueService.Models;

namespace EstoqueService.Repositories
{
    public interface IProdutoRepository
    {
        // Operações básicas de CRUD
        Task<IEnumerable<Produto>> GetAllAsync();
        Task<Produto?> GetByIdAsync(int id);
        Task<Produto> CreateAsync(Produto produto);
        Task<Produto> UpdateAsync(Produto produto);
        Task DeleteAsync(int id);

        // Consultas específicas do domínio
        Task<IEnumerable<Produto>> GetByNomeAsync(string nome);
        Task<IEnumerable<Produto>> GetByCategoriaAsync(string categoria);
        Task<IEnumerable<Produto>> GetComEstoqueAsync();
        Task<IEnumerable<Produto>> GetSemEstoqueAsync();
        Task<IEnumerable<Produto>> GetEstoqueBaixoAsync(int quantidadeMinima = 5);

        // Operações de estoque
        Task<bool> VerificarEstoqueAsync(int produtoId, int quantidade);
        Task<bool> AtualizarEstoqueAsync(int produtoId, int quantidade, string tipoMovimentacao);
    }
}