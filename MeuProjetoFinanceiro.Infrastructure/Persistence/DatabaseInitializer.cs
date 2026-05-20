using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MeuProjetoFinanceiro.Infrastructure.Persistence;

public class DatabaseInitializer : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public DatabaseInitializer(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.EnsureCreatedAsync(cancellationToken);
        await CriarTabelasDePlanejamentoAsync(context, cancellationToken);
        await GarantirFaturasItauDaPlanilhaAsync(context, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static async Task CriarTabelasDePlanejamentoAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        await context.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS "ReceitasMensais" (
                "Id" INTEGER NOT NULL CONSTRAINT "PK_ReceitasMensais" PRIMARY KEY AUTOINCREMENT,
                "Descricao" TEXT NOT NULL,
                "Valor" TEXT NOT NULL,
                "Mes" INTEGER NOT NULL,
                "Ano" INTEGER NOT NULL
            );
            """,
            cancellationToken);

        await context.Database.ExecuteSqlRawAsync(
            """
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_ReceitasMensais_Ano_Mes_Descricao"
            ON "ReceitasMensais" ("Ano", "Mes", "Descricao");
            """,
            cancellationToken);

        await context.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS "LancamentosCartaoCredito" (
                "Id" INTEGER NOT NULL CONSTRAINT "PK_LancamentosCartaoCredito" PRIMARY KEY AUTOINCREMENT,
                "Descricao" TEXT NOT NULL,
                "Cartao" TEXT NOT NULL,
                "ValorTotal" TEXT NOT NULL,
                "QuantidadeParcelas" INTEGER NOT NULL,
                "MesInicial" INTEGER NOT NULL,
                "AnoInicial" INTEGER NOT NULL,
                "Observacao" TEXT NULL,
                "CriadoEm" TEXT NOT NULL
            );
            """,
            cancellationToken);

        await context.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS "FaturasCartaoCredito" (
                "Id" INTEGER NOT NULL CONSTRAINT "PK_FaturasCartaoCredito" PRIMARY KEY AUTOINCREMENT,
                "Cartao" TEXT NOT NULL,
                "ValorTotal" TEXT NOT NULL,
                "Mes" INTEGER NOT NULL,
                "Ano" INTEGER NOT NULL,
                "Vencimento" TEXT NULL,
                "Observacao" TEXT NULL
            );
            """,
            cancellationToken);

        await context.Database.ExecuteSqlRawAsync(
            """
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_FaturasCartaoCredito_Cartao_Ano_Mes"
            ON "FaturasCartaoCredito" ("Cartao", "Ano", "Mes");
            """,
            cancellationToken);
    }

    private static async Task GarantirFaturasItauDaPlanilhaAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        var faturas = new (int Ano, int Mes, decimal Valor)[]
        {
            (2025, 12, 7000.00m),
            (2026, 1, 10994.37m),
            (2026, 2, 6403.40m),
            (2026, 3, 10897.29m),
            (2026, 4, 9779.51m),
            (2026, 5, 12017.73m),
            (2026, 6, 38234.40m),
            (2026, 7, 3699.51m),
            (2026, 8, 699.51m),
            (2026, 9, 699.51m),
            (2026, 10, 699.51m),
            (2026, 11, 519.51m),
            (2026, 12, 519.51m),
            (2027, 6, 519.51m)
        };

        foreach (var fatura in faturas)
        {
            await context.Database.ExecuteSqlRawAsync(
                """
                INSERT INTO "FaturasCartaoCredito" ("Cartao", "ValorTotal", "Mes", "Ano", "Vencimento", "Observacao")
                SELECT 'ITAU', {0}, {1}, {2}, {3}, 'Fatura informada pela planilha'
                WHERE NOT EXISTS (
                    SELECT 1 FROM "FaturasCartaoCredito"
                    WHERE "Cartao" = 'ITAU' AND "Mes" = {1} AND "Ano" = {2}
                );
                """,
                [fatura.Valor, fatura.Mes, fatura.Ano, new DateTime(fatura.Ano, fatura.Mes, 20)],
                cancellationToken);
        }
    }
}
