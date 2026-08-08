using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace MeuProjetoFinanceiro.Infrastructure.Persistence;

public class DatabaseInitializer : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DatabaseInitializer> _logger;
    private Task? _initializationTask;

    public DatabaseInitializer(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<DatabaseInitializer> logger)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _initializationTask = Task.Run(() => InicializarAsync(cancellationToken), cancellationToken);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_initializationTask is null)
        {
            return;
        }

        await Task.WhenAny(_initializationTask, Task.Delay(TimeSpan.FromSeconds(5), cancellationToken));
    }

    private async Task InicializarAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(60));

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            _logger.LogInformation("Iniciando preparacao do banco de dados.");
            await CriarSchemaPostgresAsync(context, timeout.Token);
            if (context.Database.IsNpgsql())
            {
                _logger.LogInformation("Banco PostgreSQL detectado. Pulando EnsureCreated automatico em runtime.");
                return;
            }

            await context.Database.EnsureCreatedAsync(timeout.Token);
            if (context.Database.IsSqlite())
            {
                await CriarTabelasDePlanejamentoAsync(context, timeout.Token);
            }

            await GarantirFaturasItauDaPlanilhaAsync(context, timeout.Token);
            _logger.LogInformation("Preparacao do banco de dados finalizada.");
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Preparacao do banco de dados excedeu o tempo limite e continuara na proxima inicializacao.");
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Falha ao preparar o banco de dados.");
        }
    }

    private async Task CriarSchemaPostgresAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        var schema = _configuration["Database:Schema"];
        if (string.IsNullOrWhiteSpace(schema) || !context.Database.IsNpgsql())
        {
            return;
        }

        if (!Regex.IsMatch(schema, "^[A-Za-z_][A-Za-z0-9_]*$"))
        {
            throw new InvalidOperationException("Database:Schema deve conter apenas letras, numeros e underscore, e nao pode comecar com numero.");
        }

        var schemaSeguro = schema.Replace("\"", "\"\"");
        await context.Database.ExecuteSqlRawAsync("CREATE SCHEMA IF NOT EXISTS \"" + schemaSeguro + "\";", cancellationToken);
    }

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
        var faturas = new (string Cartao, int Ano, int Mes, decimal Valor, int DiaVencimento)[]
        {
            ("ITAU UNICLASS", 2025, 12, 7000.00m, 20),
            ("ITAU UNICLASS", 2026, 1, 10994.37m, 20),
            ("ITAU UNICLASS", 2026, 2, 6403.40m, 20),
            ("ITAU UNICLASS", 2026, 3, 10897.29m, 20),
            ("ITAU UNICLASS", 2026, 4, 9779.51m, 20),
            ("ITAU UNICLASS", 2026, 5, 13969.47m, 15),
            ("ITAU UNICLASS", 2026, 6, 20241.00m, 15),
            ("ITAU UNICLASS", 2026, 7, 3193.92m, 15),
            ("ITAU UNICLASS", 2026, 8, 821.06m, 15),
            ("ITAU UNICLASS", 2026, 9, 821.06m, 15),
            ("ITAU UNICLASS", 2026, 10, 821.06m, 15),
            ("ITAU UNICLASS", 2026, 11, 557.52m, 15),
            ("ITAU UNICLASS", 2026, 12, 519.52m, 15),
            ("VISA", 2026, 5, 4263.15m, 18),
            ("VISA", 2026, 6, 484.73m, 18)
        };

        foreach (var fatura in faturas)
        {
            var existe = await context.FaturasCartaoCredito.AnyAsync(item =>
                item.Cartao == fatura.Cartao && item.Mes == fatura.Mes && item.Ano == fatura.Ano,
                cancellationToken);

            if (existe)
            {
                continue;
            }

            context.FaturasCartaoCredito.Add(new Core.Entities.FaturaCartaoCredito
            {
                Cartao = fatura.Cartao,
                ValorTotal = fatura.Valor,
                Mes = fatura.Mes,
                Ano = fatura.Ano,
                Vencimento = new DateTime(fatura.Ano, fatura.Mes, fatura.DiaVencimento),
                Observacao = "Fatura real informada pelo app do banco"
            });
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
