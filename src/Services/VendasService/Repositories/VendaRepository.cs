// Implementação do repositório para operações com vendas
// Contém a lógica de acesso aos dados utilizando Entity Framework

using Microsoft.EntityFrameworkCore;
using VendasService.Data;
using VendasService.Models;

namespace VendasService.Repositories
{
    public class VendaRepository : IVendaRepository
    {
        private readonly VendasDbContext _context;
        private readonly ILogger<VendaRepository> _logger;

        public VendaRepository(VendasDbContext context, ILogger<VendaRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Retorna todas as vendas ordenadas por data
        public async Task<IEnumerable<Venda>> GetAllAsync()
        {
            _logger.LogInformation("Consultando todas as vendas");
            return await _context.Vendas
                .OrderByDescending(v => v.DataVenda)
                .ToListAsync();
        }

        // Busca uma venda específica por ID
        public async Task<Venda?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Consultando venda com ID: {VendaId}", id);
            return await _context.Vendas.FindAsync(id);
        }

        // Cria uma nova venda
        public async Task<Venda> CreateAsync(Venda venda)
        {
            _logger.LogInformation("Criando nova venda para cliente: {Cliente}", venda.Cliente);
            
            venda.DataVenda = DateTime.UtcNow;
            venda.CalcularValorTotal();
            
            _context.Vendas.Add(venda);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Venda criada com ID: {VendaId}", venda.Id);
            return venda;
        }

        // Atualiza uma venda existente
        public async Task<Venda> UpdateAsync(Venda venda)
        {
            _logger.LogInformation("Atualizando venda com ID: {VendaId}", venda.Id);
            
            _context.Entry(venda).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            
            return venda;
        }

        // Remove uma venda
        public async Task DeleteAsync(int id)
        {
            _logger.LogInformation("Removendo venda com ID: {VendaId}", id);
            
            var venda = await _context.Vendas.FindAsync(id);
            if (venda != null)
            {
                _context.Vendas.Remove(venda);
                await _context.SaveChangesAsync();
            }
        }

        // Busca vendas por cliente
        public async Task<IEnumerable<Venda>> GetByClienteAsync(string cliente)
        {
            _logger.LogInformation("Consultando vendas do cliente: {Cliente}", cliente);
            return await _context.Vendas
                .Where(v => v.Cliente.Contains(cliente))
                .OrderByDescending(v => v.DataVenda)
                .ToListAsync();
        }

        // Busca vendas por produto
        public async Task<IEnumerable<Venda>> GetByProdutoIdAsync(int produtoId)
        {
            _logger.LogInformation("Consultando vendas do produto: {ProdutoId}", produtoId);
            return await _context.Vendas
                .Where(v => v.ProdutoId == produtoId)
                .OrderByDescending(v => v.DataVenda)
                .ToListAsync();
        }

        // Busca vendas por período
        public async Task<IEnumerable<Venda>> GetByPeriodoAsync(DateTime dataInicio, DateTime dataFim)
        {
            _logger.LogInformation("Consultando vendas do período: {DataInicio} até {DataFim}", 
                dataInicio, dataFim);
            
            return await _context.Vendas
                .Where(v => v.DataVenda >= dataInicio && v.DataVenda <= dataFim)
                .OrderByDescending(v => v.DataVenda)
                .ToListAsync();
        }

        // Busca vendas por status
        public async Task<IEnumerable<Venda>> GetByStatusAsync(string status)
        {
            _logger.LogInformation("Consultando vendas com status: {Status}", status);
            return await _context.Vendas
                .Where(v => v.Status == status)
                .OrderByDescending(v => v.DataVenda)
                .ToListAsync();
        }

        // Calcula total de vendas por período
        public async Task<decimal> GetTotalVendasPorPeriodoAsync(DateTime dataInicio, DateTime dataFim)
        {
            _logger.LogInformation("Calculando total de vendas do período: {DataInicio} até {DataFim}", 
                dataInicio, dataFim);
            
            return await _context.Vendas
                .Where(v => v.DataVenda >= dataInicio && v.DataVenda <= dataFim)
                .SumAsync(v => v.ValorTotal);
        }

        // Conta quantidade de vendas por produto em um período
        public async Task<int> GetQuantidadeVendasPorProdutoAsync(int produtoId, DateTime dataInicio, DateTime dataFim)
        {
            _logger.LogInformation("Calculando quantidade de vendas do produto {ProdutoId} no período: {DataInicio} até {DataFim}", 
                produtoId, dataInicio, dataFim);
            
            return await _context.Vendas
                .Where(v => v.ProdutoId == produtoId && 
                           v.DataVenda >= dataInicio && 
                           v.DataVenda <= dataFim)
                .SumAsync(v => v.Quantidade);
        }
    }
}