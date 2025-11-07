// Controller responsável pelos endpoints de vendas
// Expõe as operações do serviço de vendas via API REST

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Constants;
using SharedLibrary.DTOs;
using VendasService.Services;

namespace VendasService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Requer autenticação para todos os endpoints
    public class VendasController : ControllerBase
    {
        private readonly IVendaService _vendaService;
        private readonly ILogger<VendasController> _logger;

        public VendasController(IVendaService vendaService, ILogger<VendasController> logger)
        {
            _vendaService = vendaService;
            _logger = logger;
        }

        // GET: api/vendas
        // Retorna todas as vendas
        [HttpGet]
        [Authorize(Policy = "VendedorOuAdmin")]
        public async Task<ActionResult<ApiResponse<IEnumerable<VendaDto>>>> GetVendas()
        {
            _logger.LogInformation("Solicitação para buscar todas as vendas");
            var resultado = await _vendaService.GetAllVendasAsync();
            
            return resultado.Sucesso ? Ok(resultado) : BadRequest(resultado);
        }

        // GET: api/vendas/{id}
        // Retorna uma venda específica por ID
        [HttpGet("{id}")]
        [Authorize(Policy = "VendedorOuAdmin")]
        public async Task<ActionResult<ApiResponse<VendaDto>>> GetVenda(int id)
        {
            _logger.LogInformation("Solicitação para buscar venda com ID: {VendaId}", id);
            var resultado = await _vendaService.GetVendaByIdAsync(id);
            
            if (!resultado.Sucesso)
            {
                return NotFound(resultado);
            }
            
            return Ok(resultado);
        }

        // POST: api/vendas
        // Cria uma nova venda
        [HttpPost]
        [Authorize(Policy = "VendedorOuAdmin")]
        public async Task<ActionResult<ApiResponse<VendaDto>>> CreateVenda([FromBody] CriarVendaDto criarVendaDto)
        {
            _logger.LogInformation("Solicitação para criar nova venda para cliente: {Cliente}", criarVendaDto.Cliente);
            
            if (!ModelState.IsValid)
            {
                var erros = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(new ApiResponse<VendaDto>
                {
                    Sucesso = false,
                    Mensagem = "Dados inválidos",
                    Erros = erros
                });
            }

            var resultado = await _vendaService.CreateVendaAsync(criarVendaDto);
            
            if (!resultado.Sucesso)
            {
                return BadRequest(resultado);
            }
            
            return CreatedAtAction(nameof(GetVenda), new { id = resultado.Dados!.Id }, resultado);
        }

        // PUT: api/vendas/{id}
        // Atualiza uma venda existente
        [HttpPut("{id}")]
        [Authorize(Policy = "VendedorOuAdmin")]
        public async Task<ActionResult<ApiResponse<VendaDto>>> UpdateVenda(int id, [FromBody] VendaDto vendaDto)
        {
            _logger.LogInformation("Solicitação para atualizar venda com ID: {VendaId}", id);
            
            if (!ModelState.IsValid)
            {
                var erros = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(new ApiResponse<VendaDto>
                {
                    Sucesso = false,
                    Mensagem = "Dados inválidos",
                    Erros = erros
                });
            }

            var resultado = await _vendaService.UpdateVendaAsync(id, vendaDto);
            
            if (!resultado.Sucesso)
            {
                return resultado.Erros.Any(e => e.Contains("não encontrada")) 
                    ? NotFound(resultado) 
                    : BadRequest(resultado);
            }
            
            return Ok(resultado);
        }

        // DELETE: api/vendas/{id}
        // Remove uma venda
        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteVenda(int id)
        {
            _logger.LogInformation("Solicitação para remover venda com ID: {VendaId}", id);
            var resultado = await _vendaService.DeleteVendaAsync(id);
            
            if (!resultado.Sucesso)
            {
                return resultado.Erros.Any(e => e.Contains("não encontrada")) 
                    ? NotFound(resultado) 
                    : BadRequest(resultado);
            }
            
            return Ok(resultado);
        }

        // GET: api/vendas/cliente/{cliente}
        // Busca vendas por cliente
        [HttpGet("cliente/{cliente}")]
        [Authorize(Policy = "VendedorOuAdmin")]
        public async Task<ActionResult<ApiResponse<IEnumerable<VendaDto>>>> GetVendasByCliente(string cliente)
        {
            _logger.LogInformation("Solicitação para buscar vendas do cliente: {Cliente}", cliente);
            var resultado = await _vendaService.GetVendasByClienteAsync(cliente);
            
            return Ok(resultado);
        }

        // GET: api/vendas/produto/{produtoId}
        // Busca vendas por produto
        [HttpGet("produto/{produtoId}")]
        [Authorize(Policy = "VendedorOuAdmin")]
        public async Task<ActionResult<ApiResponse<IEnumerable<VendaDto>>>> GetVendasByProduto(int produtoId)
        {
            _logger.LogInformation("Solicitação para buscar vendas do produto: {ProdutoId}", produtoId);
            var resultado = await _vendaService.GetVendasByProdutoAsync(produtoId);
            
            return Ok(resultado);
        }

        // GET: api/vendas/periodo
        // Busca vendas por período
        [HttpGet("periodo")]
        [Authorize(Policy = "VendedorOuAdmin")]
        public async Task<ActionResult<ApiResponse<IEnumerable<VendaDto>>>> GetVendasByPeriodo(
            [FromQuery] DateTime dataInicio, 
            [FromQuery] DateTime dataFim)
        {
            _logger.LogInformation("Solicitação para buscar vendas do período: {DataInicio} até {DataFim}", 
                dataInicio, dataFim);
            
            if (dataInicio > dataFim)
            {
                return BadRequest(new ApiResponse<IEnumerable<VendaDto>>
                {
                    Sucesso = false,
                    Mensagem = "Data início não pode ser maior que data fim",
                    Erros = new List<string> { "Período inválido" }
                });
            }

            var resultado = await _vendaService.GetVendasByPeriodoAsync(dataInicio, dataFim);
            return Ok(resultado);
        }

        // GET: api/vendas/relatorio
        // Gera relatório de vendas por período
        [HttpGet("relatorio")]
        [Authorize(Policy = "VendedorOuAdmin")]
        public async Task<ActionResult<ApiResponse<object>>> GetRelatorioVendas(
            [FromQuery] DateTime dataInicio, 
            [FromQuery] DateTime dataFim)
        {
            _logger.LogInformation("Solicitação para gerar relatório de vendas do período: {DataInicio} até {DataFim}", 
                dataInicio, dataFim);
            
            if (dataInicio > dataFim)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Sucesso = false,
                    Mensagem = "Data início não pode ser maior que data fim",
                    Erros = new List<string> { "Período inválido" }
                });
            }

            var resultado = await _vendaService.GetRelatorioVendasAsync(dataInicio, dataFim);
            return Ok(resultado);
        }

        // GET: api/vendas/total
        // Calcula total de vendas por período
        [HttpGet("total")]
        [Authorize(Policy = "VendedorOuAdmin")]
        public async Task<ActionResult<ApiResponse<decimal>>> GetTotalVendasPorPeriodo(
            [FromQuery] DateTime dataInicio, 
            [FromQuery] DateTime dataFim)
        {
            _logger.LogInformation("Solicitação para calcular total de vendas do período: {DataInicio} até {DataFim}", 
                dataInicio, dataFim);
            
            if (dataInicio > dataFim)
            {
                return BadRequest(new ApiResponse<decimal>
                {
                    Sucesso = false,
                    Mensagem = "Data início não pode ser maior que data fim",
                    Erros = new List<string> { "Período inválido" }
                });
            }

            var resultado = await _vendaService.GetTotalVendasPorPeriodoAsync(dataInicio, dataFim);
            return Ok(resultado);
        }
    }
}