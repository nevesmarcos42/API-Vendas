// Interface para serviços de negócio de vendas
// Define as operações de alto nível para o domínio de vendas

using SharedLibrary.DTOs;

namespace VendasService.Services
{
    public interface IVendaService
    {
        // Operações principais de vendas
        Task<ApiResponse<IEnumerable<VendaDto>>> GetAllVendasAsync();
        Task<ApiResponse<VendaDto>> GetVendaByIdAsync(int id);
        Task<ApiResponse<VendaDto>> CreateVendaAsync(CriarVendaDto criarVendaDto);
        Task<ApiResponse<VendaDto>> UpdateVendaAsync(int id, VendaDto vendaDto);
        Task<ApiResponse<bool>> DeleteVendaAsync(int id);

        // Consultas específicas
        Task<ApiResponse<IEnumerable<VendaDto>>> GetVendasByClienteAsync(string cliente);
        Task<ApiResponse<IEnumerable<VendaDto>>> GetVendasByProdutoAsync(int produtoId);
        Task<ApiResponse<IEnumerable<VendaDto>>> GetVendasByPeriodoAsync(DateTime dataInicio, DateTime dataFim);
        
        // Relatórios
        Task<ApiResponse<decimal>> GetTotalVendasPorPeriodoAsync(DateTime dataInicio, DateTime dataFim);
        Task<ApiResponse<object>> GetRelatorioVendasAsync(DateTime dataInicio, DateTime dataFim);
    }
}