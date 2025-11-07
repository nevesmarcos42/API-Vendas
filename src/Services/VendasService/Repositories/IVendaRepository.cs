// Interface para operações de repositório de vendas
// Define o contrato para acesso aos dados de vendas

using VendasService.Models;

namespace VendasService.Repositories
{
    public interface IVendaRepository
    {
        // Operações básicas de CRUD
        Task<IEnumerable<Venda>> GetAllAsync();
        Task<Venda?> GetByIdAsync(int id);
        Task<Venda> CreateAsync(Venda venda);
        Task<Venda> UpdateAsync(Venda venda);
        Task DeleteAsync(int id);

        // Consultas específicas do domínio
        Task<IEnumerable<Venda>> GetByClienteAsync(string cliente);
        Task<IEnumerable<Venda>> GetByProdutoIdAsync(int produtoId);
        Task<IEnumerable<Venda>> GetByPeriodoAsync(DateTime dataInicio, DateTime dataFim);
        Task<IEnumerable<Venda>> GetByStatusAsync(string status);
        
        // Operações de relatórios
        Task<decimal> GetTotalVendasPorPeriodoAsync(DateTime dataInicio, DateTime dataFim);
        Task<int> GetQuantidadeVendasPorProdutoAsync(int produtoId, DateTime dataInicio, DateTime dataFim);
    }
}