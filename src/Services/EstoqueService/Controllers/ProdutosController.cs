using EstoqueService.Models;
using EstoqueService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace EstoqueService.Controllers
{
    [Authorize] // Requer autenticação para todos os endpoints
    [ApiController]
    [Route("api/[controller]")]
    public class ProdutosController : ControllerBase
    {
        private readonly IProdutoService _produtoService;

        public ProdutosController(IProdutoService produtoService)
        {
            _produtoService = produtoService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Produto>>> GetAll()
        {
            var produtos = await _produtoService.GetAllAsync();
            return Ok(produtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Produto>> GetById(int id)
        {
            var produto = await _produtoService.GetByIdAsync(id);
            if (produto == null)
                return NotFound();

            return Ok(produto);
        }

        [HttpPost]
        public async Task<ActionResult<Produto>> Create(Produto produto)
        {
            var createdProduto = await _produtoService.CreateAsync(produto);
            return CreatedAtAction(nameof(GetById), new { id = createdProduto.Id }, createdProduto);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Produto>> Update(int id, Produto produto)
        {
            var updatedProduto = await _produtoService.UpdateAsync(id, produto);
            if (updatedProduto == null)
                return NotFound();

            return Ok(updatedProduto);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var deleted = await _produtoService.DeleteAsync(id);
            if (!deleted)
                return NotFound();

            return NoContent();
        }

        [HttpPatch("{id}/estoque")]
        public async Task<ActionResult> UpdateEstoque(int id, [FromBody] int quantidade)
        {
            var updated = await _produtoService.UpdateEstoqueAsync(id, quantidade);
            if (!updated)
                return BadRequest("Produto não encontrado ou estoque insuficiente");

            return Ok();
        }
    }
}