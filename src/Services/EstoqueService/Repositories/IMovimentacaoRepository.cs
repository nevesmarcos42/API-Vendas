// Interface para operações de repositório de movimentações de estoque
// Define o contrato para acesso aos dados de movimentações

using EstoqueService.Models;

namespace EstoqueService.Repositories
{
    public interface IMovimentacaoRepository
    {
        // Operações básicas de CRUD
        Task<IEnumerable<MovimentacaoEstoque>> GetAllAsync();
        Task<MovimentacaoEstoque?> GetByIdAsync(int id);
        Task<MovimentacaoEstoque> CreateAsync(MovimentacaoEstoque movimentacao);

        // Consultas específicas do domínio
        Task<IEnumerable<MovimentacaoEstoque>> GetByProdutoIdAsync(int produtoId);
        Task<IEnumerable<MovimentacaoEstoque>> GetByTipoAsync(string tipo);
        Task<IEnumerable<MovimentacaoEstoque>> GetByPeriodoAsync(DateTime dataInicio, DateTime dataFim);
        Task<IEnumerable<MovimentacaoEstoque>> GetByProdutoEPeriodoAsync(int produtoId, DateTime dataInicio, DateTime dataFim);

        // Operações de relatórios
        Task<int> GetTotalEntradasPorPeriodoAsync(DateTime dataInicio, DateTime dataFim);
        Task<int> GetTotalSaidasPorPeriodoAsync(DateTime dataInicio, DateTime dataFim);
        Task<IEnumerable<object>> GetRelatorioPorProdutoAsync(DateTime dataInicio, DateTime dataFim);
    }
}