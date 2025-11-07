using EstoqueService.Models;
using EstoqueService.Repositories;

namespace EstoqueService.Services
{
    public interface IProdutoService
    {
        Task<IEnumerable<Produto>> GetAllAsync();
        Task<Produto?> GetByIdAsync(int id);
        Task<Produto> CreateAsync(Produto produto);
        Task<Produto?> UpdateAsync(int id, Produto produto);
        Task<bool> DeleteAsync(int id);
        Task<bool> UpdateEstoqueAsync(int produtoId, int quantidade);
    }

    public class ProdutoService : IProdutoService
    {
        private readonly IProdutoRepository _repository;

        public ProdutoService(IProdutoRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Produto>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<Produto?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<Produto> CreateAsync(Produto produto)
        {
            produto.DataCriacao = DateTime.UtcNow;
            produto.DataAtualizacao = DateTime.UtcNow;
            return await _repository.CreateAsync(produto);
        }

        public async Task<Produto?> UpdateAsync(int id, Produto produto)
        {
            var existingProduto = await _repository.GetByIdAsync(id);
            if (existingProduto == null)
                return null;

            existingProduto.Nome = produto.Nome;
            existingProduto.Descricao = produto.Descricao;
            existingProduto.Preco = produto.Preco;
            existingProduto.QuantidadeEstoque = produto.QuantidadeEstoque;
            existingProduto.DataAtualizacao = DateTime.UtcNow;

            return await _repository.UpdateAsync(existingProduto);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var produto = await _repository.GetByIdAsync(id);
            if (produto == null)
                return false;

            await _repository.DeleteAsync(id);
            return true;
        }

        public async Task<bool> UpdateEstoqueAsync(int produtoId, int quantidade)
        {
            var produto = await _repository.GetByIdAsync(produtoId);
            if (produto == null || produto.QuantidadeEstoque < quantidade)
                return false;

            produto.QuantidadeEstoque -= quantidade;
            produto.DataAtualizacao = DateTime.UtcNow;
            
            await _repository.UpdateAsync(produto);
            return true;
        }
    }
}