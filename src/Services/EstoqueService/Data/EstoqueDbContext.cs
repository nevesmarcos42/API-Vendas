// Contexto do Entity Framework para o serviço de estoque
// Responsável pela configuração e acesso aos dados de produtos e movimentações

using Microsoft.EntityFrameworkCore;
using EstoqueService.Models;

namespace EstoqueService.Data
{
    public class EstoqueDbContext : DbContext
    {
        public EstoqueDbContext(DbContextOptions<EstoqueDbContext> options) : base(options)
        {
        }

        // DbSets representam as tabelas no banco de dados
        public DbSet<Produto> Produtos { get; set; }
        public DbSet<MovimentacaoEstoque> MovimentacoesEstoque { get; set; }

        // Configuração adicional dos modelos e relacionamentos
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuração da entidade Produto
            modelBuilder.Entity<Produto>(entity =>
            {
                // Configuração de índices para melhorar performance de consultas
                entity.HasIndex(p => p.Nome)
                      .HasDatabaseName("IX_Produtos_Nome");

                entity.HasIndex(p => p.Categoria)
                      .HasDatabaseName("IX_Produtos_Categoria");

                entity.HasIndex(p => p.DataCriacao)
                      .HasDatabaseName("IX_Produtos_DataCriacao");

                // Configuração de propriedades com valores padrão
                entity.Property(p => p.DataCriacao)
                      .HasDefaultValueSql("NOW()");

                entity.Property(p => p.QuantidadeEstoque)
                      .HasDefaultValue(0);

                // Configuração de precisão para campos decimais
                entity.Property(p => p.Preco)
                      .HasPrecision(18, 2);

                // Configuração do relacionamento com movimentações
                entity.HasMany(p => p.Movimentacoes)
                      .WithOne(m => m.Produto)
                      .HasForeignKey(m => m.ProdutoId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configuração da entidade MovimentacaoEstoque
            modelBuilder.Entity<MovimentacaoEstoque>(entity =>
            {
                // Configuração de índices
                entity.HasIndex(m => m.ProdutoId)
                      .HasDatabaseName("IX_MovimentacoesEstoque_ProdutoId");

                entity.HasIndex(m => m.DataMovimentacao)
                      .HasDatabaseName("IX_MovimentacoesEstoque_DataMovimentacao");

                entity.HasIndex(m => m.TipoMovimentacao)
                      .HasDatabaseName("IX_MovimentacoesEstoque_TipoMovimentacao");

                // Configuração de propriedades com valores padrão
                entity.Property(m => m.DataCriacao)
                      .HasDefaultValueSql("NOW()");

                entity.Property(m => m.DataMovimentacao)
                      .HasDefaultValueSql("NOW()");
            });

            // Dados iniciais para desenvolvimento
            SeedData(modelBuilder);
        }

        // Override para aplicar comportamentos automáticos antes de salvar
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        if (entry.Entity is Produto produto)
                        {
                            produto.DataCriacao = DateTime.UtcNow;
                        }
                        else if (entry.Entity is MovimentacaoEstoque movimentacao)
                        {
                            movimentacao.DataCriacao = DateTime.UtcNow;
                            movimentacao.DataMovimentacao = DateTime.UtcNow;
                        }
                        break;
                    case EntityState.Modified:
                        if (entry.Entity is Produto produtoModificado)
                        {
                            produtoModificado.DataAtualizacao = DateTime.UtcNow;
                        }
                        break;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }

        // Método para inserir dados iniciais
        private void SeedData(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Produto>().HasData(
                new Produto
                {
                    Id = 1,
                    Nome = "Notebook Dell Inspiron",
                    Descricao = "Notebook Dell Inspiron 15 com 8GB RAM e SSD 256GB",
                    Preco = 2499.99m,
                    QuantidadeEstoque = 10,
                    Categoria = "Informática",
                    DataCriacao = DateTime.UtcNow
                },
                new Produto
                {
                    Id = 2,
                    Nome = "Mouse Wireless Logitech",
                    Descricao = "Mouse sem fio Logitech com sensor óptico",
                    Preco = 89.90m,
                    QuantidadeEstoque = 25,
                    Categoria = "Periféricos",
                    DataCriacao = DateTime.UtcNow
                },
                new Produto
                {
                    Id = 3,
                    Nome = "Teclado Mecânico RGB",
                    Descricao = "Teclado mecânico com iluminação RGB e switches Cherry MX",
                    Preco = 299.99m,
                    QuantidadeEstoque = 15,
                    Categoria = "Periféricos",
                    DataCriacao = DateTime.UtcNow
                }
            );
        }
    }
}