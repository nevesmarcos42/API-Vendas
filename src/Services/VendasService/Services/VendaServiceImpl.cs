// Implementação dos serviços de negócio para vendas
// Contém a lógica de negócio e coordena operações com repositórios e mensageria

using SharedLibrary.Constants;
using SharedLibrary.DTOs;
using SharedLibrary.Interfaces;
using SharedLibrary.Messages;
using VendasService.Models;
using VendasService.Repositories;

namespace VendasService.Services
{
    public class VendaServiceImpl : IVendaService
    {
        private readonly IVendaRepository _vendaRepository;
        private readonly IMessageBus _messageBus;
        private readonly ILogger<VendaServiceImpl> _logger;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public VendaServiceImpl(
            IVendaRepository vendaRepository,
            IMessageBus messageBus,
            ILogger<VendaServiceImpl> logger,
            HttpClient httpClient,
            IConfiguration configuration)
        {
            _vendaRepository = vendaRepository;
            _messageBus = messageBus;
            _logger = logger;
            _httpClient = httpClient;
            _configuration = configuration;
        }

        // Retorna todas as vendas
        public async Task<ApiResponse<IEnumerable<VendaDto>>> GetAllVendasAsync()
        {
            try
            {
                var vendas = await _vendaRepository.GetAllAsync();
                var vendasDto = vendas.Select(MapToDto);

                return new ApiResponse<IEnumerable<VendaDto>>
                {
                    Sucesso = true,
                    Dados = vendasDto,
                    Mensagem = "Vendas recuperadas com sucesso"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar todas as vendas");
                return new ApiResponse<IEnumerable<VendaDto>>
                {
                    Sucesso = false,
                    Mensagem = "Erro interno do servidor",
                    Erros = new List<string> { "Falha ao recuperar vendas" }
                };
            }
        }

        // Busca venda por ID
        public async Task<ApiResponse<VendaDto>> GetVendaByIdAsync(int id)
        {
            try
            {
                var venda = await _vendaRepository.GetByIdAsync(id);
                
                if (venda == null)
                {
                    return new ApiResponse<VendaDto>
                    {
                        Sucesso = false,
                        Mensagem = "Venda não encontrada",
                        Erros = new List<string> { $"Venda com ID {id} não existe" }
                    };
                }

                return new ApiResponse<VendaDto>
                {
                    Sucesso = true,
                    Dados = MapToDto(venda),
                    Mensagem = "Venda encontrada com sucesso"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar venda com ID: {VendaId}", id);
                return new ApiResponse<VendaDto>
                {
                    Sucesso = false,
                    Mensagem = "Erro interno do servidor",
                    Erros = new List<string> { "Falha ao recuperar venda" }
                };
            }
        }

        // Cria uma nova venda
        public async Task<ApiResponse<VendaDto>> CreateVendaAsync(CriarVendaDto criarVendaDto)
        {
            try
            {
                // Verifica se o produto existe e tem estoque suficiente
                var produtoInfo = await VerificarProdutoAsync(criarVendaDto.ProdutoId);
                if (produtoInfo == null)
                {
                    return new ApiResponse<VendaDto>
                    {
                        Sucesso = false,
                        Mensagem = "Produto não encontrado",
                        Erros = new List<string> { $"Produto com ID {criarVendaDto.ProdutoId} não existe" }
                    };
                }

                if (produtoInfo.QuantidadeEstoque < criarVendaDto.Quantidade)
                {
                    return new ApiResponse<VendaDto>
                    {
                        Sucesso = false,
                        Mensagem = "Estoque insuficiente",
                        Erros = new List<string> { $"Estoque disponível: {produtoInfo.QuantidadeEstoque}" }
                    };
                }

                // Cria a venda
                var venda = new Venda
                {
                    ProdutoId = criarVendaDto.ProdutoId,
                    NomeProduto = produtoInfo.Nome,
                    Quantidade = criarVendaDto.Quantidade,
                    PrecoUnitario = produtoInfo.Preco,
                    Cliente = criarVendaDto.Cliente,
                    Status = StatusVenda.Pendente
                };

                var vendaCriada = await _vendaRepository.CreateAsync(venda);

                // Publica evento de venda registrada para atualizar estoque
                var vendaEvent = new VendaRegistradaEvent
                {
                    VendaId = vendaCriada.Id,
                    ProdutoId = vendaCriada.ProdutoId,
                    Quantidade = vendaCriada.Quantidade,
                    Cliente = vendaCriada.Cliente,
                    DataVenda = vendaCriada.DataVenda,
                    ValorTotal = vendaCriada.ValorTotal
                };

                await _messageBus.PublishAsync(QueueNames.VendaRegistrada, vendaEvent);

                return new ApiResponse<VendaDto>
                {
                    Sucesso = true,
                    Dados = MapToDto(vendaCriada),
                    Mensagem = "Venda criada com sucesso"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar venda");
                return new ApiResponse<VendaDto>
                {
                    Sucesso = false,
                    Mensagem = "Erro interno do servidor",
                    Erros = new List<string> { "Falha ao criar venda" }
                };
            }
        }

        // Atualiza uma venda existente
        public async Task<ApiResponse<VendaDto>> UpdateVendaAsync(int id, VendaDto vendaDto)
        {
            try
            {
                var vendaExistente = await _vendaRepository.GetByIdAsync(id);
                if (vendaExistente == null)
                {
                    return new ApiResponse<VendaDto>
                    {
                        Sucesso = false,
                        Mensagem = "Venda não encontrada",
                        Erros = new List<string> { $"Venda com ID {id} não existe" }
                    };
                }

                // Atualiza apenas campos permitidos
                vendaExistente.Status = vendaDto.Status;
                
                var vendaAtualizada = await _vendaRepository.UpdateAsync(vendaExistente);

                return new ApiResponse<VendaDto>
                {
                    Sucesso = true,
                    Dados = MapToDto(vendaAtualizada),
                    Mensagem = "Venda atualizada com sucesso"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar venda com ID: {VendaId}", id);
                return new ApiResponse<VendaDto>
                {
                    Sucesso = false,
                    Mensagem = "Erro interno do servidor",
                    Erros = new List<string> { "Falha ao atualizar venda" }
                };
            }
        }

        // Remove uma venda
        public async Task<ApiResponse<bool>> DeleteVendaAsync(int id)
        {
            try
            {
                var venda = await _vendaRepository.GetByIdAsync(id);
                if (venda == null)
                {
                    return new ApiResponse<bool>
                    {
                        Sucesso = false,
                        Mensagem = "Venda não encontrada",
                        Erros = new List<string> { $"Venda com ID {id} não existe" }
                    };
                }

                await _vendaRepository.DeleteAsync(id);

                return new ApiResponse<bool>
                {
                    Sucesso = true,
                    Dados = true,
                    Mensagem = "Venda removida com sucesso"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao remover venda com ID: {VendaId}", id);
                return new ApiResponse<bool>
                {
                    Sucesso = false,
                    Mensagem = "Erro interno do servidor",
                    Erros = new List<string> { "Falha ao remover venda" }
                };
            }
        }

        // Busca vendas por cliente
        public async Task<ApiResponse<IEnumerable<VendaDto>>> GetVendasByClienteAsync(string cliente)
        {
            try
            {
                var vendas = await _vendaRepository.GetByClienteAsync(cliente);
                var vendasDto = vendas.Select(MapToDto);

                return new ApiResponse<IEnumerable<VendaDto>>
                {
                    Sucesso = true,
                    Dados = vendasDto,
                    Mensagem = $"Vendas do cliente {cliente} recuperadas com sucesso"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar vendas do cliente: {Cliente}", cliente);
                return new ApiResponse<IEnumerable<VendaDto>>
                {
                    Sucesso = false,
                    Mensagem = "Erro interno do servidor",
                    Erros = new List<string> { "Falha ao recuperar vendas do cliente" }
                };
            }
        }

        // Busca vendas por produto
        public async Task<ApiResponse<IEnumerable<VendaDto>>> GetVendasByProdutoAsync(int produtoId)
        {
            try
            {
                var vendas = await _vendaRepository.GetByProdutoIdAsync(produtoId);
                var vendasDto = vendas.Select(MapToDto);

                return new ApiResponse<IEnumerable<VendaDto>>
                {
                    Sucesso = true,
                    Dados = vendasDto,
                    Mensagem = $"Vendas do produto {produtoId} recuperadas com sucesso"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar vendas do produto: {ProdutoId}", produtoId);
                return new ApiResponse<IEnumerable<VendaDto>>
                {
                    Sucesso = false,
                    Mensagem = "Erro interno do servidor",
                    Erros = new List<string> { "Falha ao recuperar vendas do produto" }
                };
            }
        }

        // Busca vendas por período
        public async Task<ApiResponse<IEnumerable<VendaDto>>> GetVendasByPeriodoAsync(DateTime dataInicio, DateTime dataFim)
        {
            try
            {
                var vendas = await _vendaRepository.GetByPeriodoAsync(dataInicio, dataFim);
                var vendasDto = vendas.Select(MapToDto);

                return new ApiResponse<IEnumerable<VendaDto>>
                {
                    Sucesso = true,
                    Dados = vendasDto,
                    Mensagem = "Vendas do período recuperadas com sucesso"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar vendas do período: {DataInicio} até {DataFim}", dataInicio, dataFim);
                return new ApiResponse<IEnumerable<VendaDto>>
                {
                    Sucesso = false,
                    Mensagem = "Erro interno do servidor",
                    Erros = new List<string> { "Falha ao recuperar vendas do período" }
                };
            }
        }

        // Calcula total de vendas por período
        public async Task<ApiResponse<decimal>> GetTotalVendasPorPeriodoAsync(DateTime dataInicio, DateTime dataFim)
        {
            try
            {
                var total = await _vendaRepository.GetTotalVendasPorPeriodoAsync(dataInicio, dataFim);

                return new ApiResponse<decimal>
                {
                    Sucesso = true,
                    Dados = total,
                    Mensagem = "Total de vendas calculado com sucesso"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao calcular total de vendas do período: {DataInicio} até {DataFim}", dataInicio, dataFim);
                return new ApiResponse<decimal>
                {
                    Sucesso = false,
                    Mensagem = "Erro interno do servidor",
                    Erros = new List<string> { "Falha ao calcular total de vendas" }
                };
            }
        }

        // Gera relatório completo de vendas
        public async Task<ApiResponse<object>> GetRelatorioVendasAsync(DateTime dataInicio, DateTime dataFim)
        {
            try
            {
                var vendas = await _vendaRepository.GetByPeriodoAsync(dataInicio, dataFim);
                var total = await _vendaRepository.GetTotalVendasPorPeriodoAsync(dataInicio, dataFim);

                var relatorio = new
                {
                    Periodo = new { DataInicio = dataInicio, DataFim = dataFim },
                    TotalVendas = vendas.Count(),
                    ValorTotal = total,
                    VendasPorStatus = vendas.GroupBy(v => v.Status)
                        .Select(g => new { Status = g.Key, Quantidade = g.Count(), Valor = g.Sum(v => v.ValorTotal) }),
                    TopClientes = vendas.GroupBy(v => v.Cliente)
                        .Select(g => new { Cliente = g.Key, TotalCompras = g.Sum(v => v.ValorTotal) })
                        .OrderByDescending(c => c.TotalCompras)
                        .Take(10)
                };

                return new ApiResponse<object>
                {
                    Sucesso = true,
                    Dados = relatorio,
                    Mensagem = "Relatório gerado com sucesso"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar relatório de vendas");
                return new ApiResponse<object>
                {
                    Sucesso = false,
                    Mensagem = "Erro interno do servidor",
                    Erros = new List<string> { "Falha ao gerar relatório" }
                };
            }
        }

        // Verifica informações do produto no serviço de estoque
        private async Task<ProdutoDto?> VerificarProdutoAsync(int produtoId)
        {
            try
            {
                var estoqueServiceUrl = _configuration["ServiceEndpoints:EstoqueServiceUrl"];
                var response = await _httpClient.GetAsync($"{estoqueServiceUrl}/api/produtos/{produtoId}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiResponse = System.Text.Json.JsonSerializer.Deserialize<ApiResponse<ProdutoDto>>(content);
                    return apiResponse?.Dados;
                }
                
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar produto no serviço de estoque");
                return null;
            }
        }

        // Mapeia entidade para DTO
        private static VendaDto MapToDto(Venda venda)
        {
            return new VendaDto
            {
                Id = venda.Id,
                ProdutoId = venda.ProdutoId,
                NomeProduto = venda.NomeProduto,
                Quantidade = venda.Quantidade,
                PrecoUnitario = venda.PrecoUnitario,
                ValorTotal = venda.ValorTotal,
                Cliente = venda.Cliente,
                DataVenda = venda.DataVenda,
                Status = venda.Status
            };
        }
    }
}