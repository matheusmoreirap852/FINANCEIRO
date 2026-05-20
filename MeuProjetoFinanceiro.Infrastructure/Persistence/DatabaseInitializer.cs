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
    }
}
