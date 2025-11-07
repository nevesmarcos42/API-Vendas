// Implementação do repositório para operações com produtos
// Contém a lógica de acesso aos dados utilizando Entity Framework

using Microsoft.EntityFrameworkCore;
using EstoqueService.Data;
using EstoqueService.Models;

namespace EstoqueService.Repositories
{
    public class ProdutoRepository : IProdutoRepository
    {
        private readonly EstoqueDbContext _context;
        private readonly ILogger<ProdutoRepository> _logger;

        public ProdutoRepository(EstoqueDbContext context, ILogger<ProdutoRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Retorna todos os produtos ordenados por nome
        public async Task<IEnumerable<Produto>> GetAllAsync()
        {
            _logger.LogInformation("Consultando todos os produtos");
            return await _context.Produtos
                .OrderBy(p => p.Nome)
                .ToListAsync();
        }

        // Busca um produto específico por ID
        public async Task<Produto?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Consultando produto com ID: {ProdutoId}", id);
            return await _context.Produtos
                .Include(p => p.Movimentacoes)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        // Cria um novo produto
        public async Task<Produto> CreateAsync(Produto produto)
        {
            _logger.LogInformation("Criando novo produto: {NomeProduto}", produto.Nome);
            
            _context.Produtos.Add(produto);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Produto criado com ID: {ProdutoId}", produto.Id);
            return produto;
        }

        // Atualiza um produto existente
        public async Task<Produto> UpdateAsync(Produto produto)
        {
            _logger.LogInformation("Atualizando produto com ID: {ProdutoId}", produto.Id);
            
            _context.Entry(produto).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            
            return produto;
        }

        // Remove um produto
        public async Task DeleteAsync(int id)
        {
            _logger.LogInformation("Removendo produto com ID: {ProdutoId}", id);
            
            var produto = await _context.Produtos.FindAsync(id);
            if (produto != null)
            {
                _context.Produtos.Remove(produto);
                await _context.SaveChangesAsync();
            }
        }

        // Busca produtos por nome
        public async Task<IEnumerable<Produto>> GetByNomeAsync(string nome)
        {
            _logger.LogInformation("Consultando produtos com nome: {Nome}", nome);
            return await _context.Produtos
                .Where(p => p.Nome.Contains(nome))
                .OrderBy(p => p.Nome)
                .ToListAsync();
        }

        // Busca produtos por categoria
        public async Task<IEnumerable<Produto>> GetByCategoriaAsync(string categoria)
        {
            _logger.LogInformation("Consultando produtos da categoria: {Categoria}", categoria);
            return await _context.Produtos
                .Where(p => p.Categoria == categoria)
                .OrderBy(p => p.Nome)
                .ToListAsync();
        }

        // Busca produtos com estoque disponível
        public async Task<IEnumerable<Produto>> GetComEstoqueAsync()
        {
            _logger.LogInformation("Consultando produtos com estoque disponível");
            return await _context.Produtos
                .Where(p => p.QuantidadeEstoque > 0)
                .OrderBy(p => p.Nome)
                .ToListAsync();
        }

        // Busca produtos sem estoque
        public async Task<IEnumerable<Produto>> GetSemEstoqueAsync()
        {
            _logger.LogInformation("Consultando produtos sem estoque");
            return await _context.Produtos
                .Where(p => p.QuantidadeEstoque == 0)
                .OrderBy(p => p.Nome)
                .ToListAsync();
        }

        // Busca produtos com estoque baixo
        public async Task<IEnumerable<Produto>> GetEstoqueBaixoAsync(int quantidadeMinima = 5)
        {
            _logger.LogInformation("Consultando produtos com estoque baixo (menor que {QuantidadeMinima})", quantidadeMinima);
            return await _context.Produtos
                .Where(p => p.QuantidadeEstoque <= quantidadeMinima && p.QuantidadeEstoque > 0)
                .OrderBy(p => p.QuantidadeEstoque)
                .ToListAsync();
        }

        // Verifica se há estoque suficiente para um produto
        public async Task<bool> VerificarEstoqueAsync(int produtoId, int quantidade)
        {
            _logger.LogInformation("Verificando estoque do produto {ProdutoId} para quantidade {Quantidade}", 
                produtoId, quantidade);
            
            var produto = await _context.Produtos.FindAsync(produtoId);
            return produto?.TemEstoqueSuficiente(quantidade) ?? false;
        }

        // Atualiza o estoque de um produto
        public async Task<bool> AtualizarEstoqueAsync(int produtoId, int quantidade, string tipoMovimentacao)
        {
            _logger.LogInformation("Atualizando estoque do produto {ProdutoId}: {TipoMovimentacao} de {Quantidade}", 
                produtoId, tipoMovimentacao, quantidade);
            
            var produto = await _context.Produtos.FindAsync(produtoId);
            if (produto == null)
            {
                _logger.LogWarning("Produto {ProdutoId} não encontrado para atualização de estoque", produtoId);
                return false;
            }

            var quantidadeAnterior = produto.QuantidadeEstoque;
            produto.AtualizarEstoque(quantidade, tipoMovimentacao);

            // Registra a movimentação
            var movimentacao = new MovimentacaoEstoque
            {
                ProdutoId = produtoId,
                TipoMovimentacao = tipoMovimentacao,
                Quantidade = quantidade,
                QuantidadeAnterior = quantidadeAnterior,
                QuantidadeAtual = produto.QuantidadeEstoque,
                Motivo = $"Atualização automática via {tipoMovimentacao}",
                DataMovimentacao = DateTime.UtcNow
            };

            _context.MovimentacoesEstoque.Add(movimentacao);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Estoque atualizado - Produto: {ProdutoId}, Anterior: {QuantidadeAnterior}, Atual: {QuantidadeAtual}", 
                produtoId, quantidadeAnterior, produto.QuantidadeEstoque);

            return true;
        }
    }
}