// Contexto do Entity Framework para o serviço de vendas
// Responsável pela configuração e acesso aos dados de vendas

using Microsoft.EntityFrameworkCore;
using VendasService.Models;

namespace VendasService.Data
{
    public class VendasDbContext : DbContext
    {
        public VendasDbContext(DbContextOptions<VendasDbContext> options) : base(options)
        {
        }

        // DbSets representam as tabelas no banco de dados
        public DbSet<Venda> Vendas { get; set; }

        // Configuração adicional dos modelos e relacionamentos
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuração da entidade Venda
            modelBuilder.Entity<Venda>(entity =>
            {
                // Configuração de índices para melhorar performance de consultas
                entity.HasIndex(v => v.DataVenda)
                      .HasDatabaseName("IX_Vendas_DataVenda");

                entity.HasIndex(v => v.Cliente)
                      .HasDatabaseName("IX_Vendas_Cliente");

                entity.HasIndex(v => v.ProdutoId)
                      .HasDatabaseName("IX_Vendas_ProdutoId");

                entity.HasIndex(v => v.Status)
                      .HasDatabaseName("IX_Vendas_Status");

                // Configuração de propriedades com valores padrão
                entity.Property(v => v.DataCriacao)
                      .HasDefaultValueSql("NOW()");

                entity.Property(v => v.Status)
                      .HasDefaultValue("Pendente");

                // Configuração de precisão para campos decimais
                entity.Property(v => v.PrecoUnitario)
                      .HasPrecision(18, 2);

                entity.Property(v => v.ValorTotal)
                      .HasPrecision(18, 2);
            });
        }

        // Override para aplicar comportamentos automáticos antes de salvar
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<Venda>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.DataCriacao = DateTime.UtcNow;
                        // Calcula o valor total automaticamente
                        entry.Entity.CalcularValorTotal();
                        break;
                    case EntityState.Modified:
                        entry.Entity.DataAtualizacao = DateTime.UtcNow;
                        // Recalcula o valor total em caso de alteração
                        entry.Entity.CalcularValorTotal();
                        break;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}