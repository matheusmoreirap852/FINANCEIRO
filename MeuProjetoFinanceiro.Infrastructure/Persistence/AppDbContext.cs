using MeuProjetoFinanceiro.Core.Entities;
using MeuProjetoFinanceiro.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace MeuProjetoFinanceiro.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<ContaFinanceira> ContasFinanceiras => Set<ContaFinanceira>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Transacao> Transacoes => Set<Transacao>();
    public DbSet<Orcamento> Orcamentos => Set<Orcamento>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ContaFinanceira>(entity =>
        {
            entity.ToTable("ContasFinanceiras");
            entity.Property(e => e.Nome).HasMaxLength(120).IsRequired();
            entity.Property(e => e.Instituicao).HasMaxLength(120);
            entity.Property(e => e.SaldoInicial).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.ToTable("Categorias");
            entity.Property(e => e.Nome).HasMaxLength(120).IsRequired();
            entity.Property(e => e.Cor).HasMaxLength(20);
        });

        modelBuilder.Entity<Transacao>(entity =>
        {
            entity.ToTable("Transacoes");
            entity.Property(e => e.Descricao).HasMaxLength(180).IsRequired();
            entity.Property(e => e.Observacao).HasMaxLength(500);
            entity.Property(e => e.Valor).HasPrecision(18, 2);
            entity.HasOne(e => e.ContaFinanceira)
                .WithMany(c => c.Transacoes)
                .HasForeignKey(e => e.ContaFinanceiraId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Categoria)
                .WithMany(c => c.Transacoes)
                .HasForeignKey(e => e.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Orcamento>(entity =>
        {
            entity.ToTable("Orcamentos");
            entity.Property(e => e.Nome).HasMaxLength(120).IsRequired();
            entity.Property(e => e.ValorPlanejado).HasPrecision(18, 2);
            entity.HasOne(e => e.Categoria)
                .WithMany(c => c.Orcamentos)
                .HasForeignKey(e => e.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        Seed(modelBuilder);
    }

    private static void Seed(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ContaFinanceira>().HasData(
            new ContaFinanceira { Id = 1, Nome = "Conta Principal", Instituicao = "Banco Digital", Tipo = TipoConta.Corrente, SaldoInicial = 3200m, Ativa = true, CriadaEm = new DateTime(2026, 1, 1) },
            new ContaFinanceira { Id = 2, Nome = "Reserva", Instituicao = "Investimentos", Tipo = TipoConta.Investimento, SaldoInicial = 12500m, Ativa = true, CriadaEm = new DateTime(2026, 1, 1) });

        modelBuilder.Entity<Categoria>().HasData(
            new Categoria { Id = 1, Nome = "Salario", Tipo = TipoTransacao.Receita, Cor = "#16a34a", Ativa = true },
            new Categoria { Id = 2, Nome = "Moradia", Tipo = TipoTransacao.Despesa, Cor = "#2563eb", Ativa = true },
            new Categoria { Id = 3, Nome = "Mercado", Tipo = TipoTransacao.Despesa, Cor = "#f97316", Ativa = true },
            new Categoria { Id = 4, Nome = "Transporte", Tipo = TipoTransacao.Despesa, Cor = "#7c3aed", Ativa = true },
            new Categoria { Id = 5, Nome = "Lazer", Tipo = TipoTransacao.Despesa, Cor = "#db2777", Ativa = true });

        modelBuilder.Entity<Transacao>().HasData(
            new Transacao { Id = 1, Descricao = "Pagamento mensal", Valor = 8500m, Data = new DateTime(2026, 5, 5), Tipo = TipoTransacao.Receita, Efetivada = true, ContaFinanceiraId = 1, CategoriaId = 1 },
            new Transacao { Id = 2, Descricao = "Aluguel", Valor = 2200m, Data = new DateTime(2026, 5, 7), Tipo = TipoTransacao.Despesa, Efetivada = true, ContaFinanceiraId = 1, CategoriaId = 2 },
            new Transacao { Id = 3, Descricao = "Compras do mes", Valor = 980m, Data = new DateTime(2026, 5, 10), Tipo = TipoTransacao.Despesa, Efetivada = true, ContaFinanceiraId = 1, CategoriaId = 3 },
            new Transacao { Id = 4, Descricao = "Combustivel e aplicativo", Valor = 420m, Data = new DateTime(2026, 5, 14), Tipo = TipoTransacao.Despesa, Efetivada = true, ContaFinanceiraId = 1, CategoriaId = 4 },
            new Transacao { Id = 5, Descricao = "Cinema e jantar", Valor = 260m, Data = new DateTime(2026, 5, 18), Tipo = TipoTransacao.Despesa, Efetivada = false, ContaFinanceiraId = 1, CategoriaId = 5 });

        modelBuilder.Entity<Orcamento>().HasData(
            new Orcamento { Id = 1, Nome = "Moradia Maio", Mes = 5, Ano = 2026, ValorPlanejado = 2400m, CategoriaId = 2 },
            new Orcamento { Id = 2, Nome = "Mercado Maio", Mes = 5, Ano = 2026, ValorPlanejado = 1200m, CategoriaId = 3 },
            new Orcamento { Id = 3, Nome = "Transporte Maio", Mes = 5, Ano = 2026, ValorPlanejado = 650m, CategoriaId = 4 });
    }
}
