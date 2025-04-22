using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace AtivosFinanceiros.Models;

public partial class MeuDbContext : DbContext
{
    public MeuDbContext()
    {
    }

    public MeuDbContext(DbContextOptions<MeuDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Ativo> Ativos { get; set; }

    public virtual DbSet<DepositoPrazo> DepositoPrazos { get; set; }

    public virtual DbSet<FundoInvestimento> FundoInvestimentos { get; set; }

    public virtual DbSet<ImovelArrendado> ImovelArrendados { get; set; }

    public virtual DbSet<Relatorio> Relatorios { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build();

            var connectionString = $"Host={configuration["POSTGRES_HOST"]};" +
                                   $"Port={configuration["POSTGRES_PORT"]};" +
                                   $"Database={configuration["POSTGRES_DB"]};" +
                                   $"Username={configuration["POSTGRES_USER"]};" +
                                   $"Password={configuration["POSTGRES_PASSWORD"]}";

            optionsBuilder.UseNpgsql(connectionString);
        }
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("uuid-ossp");

        modelBuilder.Entity<Ativo>(entity =>
        {
            entity.HasKey(e => e.AtivoUuid).HasName("ativos_pkey");

            entity.ToTable("ativos");

            entity.Property(e => e.AtivoUuid)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("ativo_uuid");
            entity.Property(e => e.DataInicio).HasColumnName("data_inicio");
            entity.Property(e => e.DuracaoMeses).HasColumnName("duracao_meses");
            entity.Property(e => e.ImpostoPerc)
                .HasPrecision(5, 2)
                .HasColumnName("imposto_perc");
            entity.Property(e => e.LucroTotal)
                .HasPrecision(10, 2)
                .HasColumnName("lucro_total");
            entity.Property(e => e.Nome)
                .HasMaxLength(255)
                .HasColumnName("nome");
            entity.Property(e => e.TipoAtivo)
                .HasMaxLength(50)
                .HasColumnName("tipo_ativo");
            entity.Property(e => e.UserUuid).HasColumnName("user_uuid");
            entity.Property(e => e.ValorInicial)
                .HasPrecision(10, 2)
                .HasColumnName("valor_inicial");

            entity.HasOne(d => d.User).WithMany(p => p.Ativos)
                .HasForeignKey(d => d.UserUuid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("ativos_user_uuid_fkey");
        });

        modelBuilder.Entity<DepositoPrazo>(entity =>
        {
            entity.HasKey(e => e.DepositoUuid).HasName("deposito_prazo_pkey");

            entity.ToTable("deposito_prazo");

            entity.Property(e => e.DepositoUuid)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("deposito_uuid");
            entity.Property(e => e.AtivoUuid).HasColumnName("ativo_uuid");
            entity.Property(e => e.Banco)
                .HasMaxLength(255)
                .HasColumnName("banco");
            entity.Property(e => e.NumeroConta)
                .HasMaxLength(255)
                .HasColumnName("numero_conta");
            entity.Property(e => e.TaxaAnual)
                .HasPrecision(5, 2)
                .HasColumnName("taxa_anual");
            entity.Property(e => e.Titulares)
                .HasMaxLength(255)
                .HasColumnName("titulares");
            entity.Property(e => e.ValorInicial)
                .HasPrecision(10, 2)
                .HasColumnName("valor_inicial");

            entity.HasOne(d => d.AtivoUu).WithMany(p => p.DepositoPrazos)
                .HasForeignKey(d => d.AtivoUuid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("deposito_prazo_ativo_uuid_fkey");
        });

        modelBuilder.Entity<FundoInvestimento>(entity =>
        {
            entity.HasKey(e => e.FundoUuid).HasName("fundo_investimento_pkey");

            entity.ToTable("fundo_investimento");

            entity.Property(e => e.FundoUuid)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("fundo_uuid");
            entity.Property(e => e.AtivoUuid).HasColumnName("ativo_uuid");
            entity.Property(e => e.JurosMensal)
                .HasPrecision(5, 2)
                .HasColumnName("juros_mensal");
            entity.Property(e => e.JurosPadrao)
                .HasPrecision(5, 2)
                .HasColumnName("juros_padrao");
            entity.Property(e => e.MonteInvestido)
                .HasPrecision(10, 2)
                .HasColumnName("monte_investido");

            entity.HasOne(d => d.AtivoUu).WithMany(p => p.FundoInvestimentos)
                .HasForeignKey(d => d.AtivoUuid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fundo_investimento_ativo_uuid_fkey");
        });

        modelBuilder.Entity<ImovelArrendado>(entity =>
        {
            entity.HasKey(e => e.ImovelUuid).HasName("imovel_arrendado_pkey");

            entity.ToTable("imovel_arrendado");

            entity.Property(e => e.ImovelUuid)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("imovel_uuid");
            entity.Property(e => e.AtivoUuid).HasColumnName("ativo_uuid");
            entity.Property(e => e.DespesasAnuais)
                .HasPrecision(10, 2)
                .HasColumnName("despesas_anuais");
            entity.Property(e => e.Localizacao)
                .HasMaxLength(255)
                .HasColumnName("localizacao");
            entity.Property(e => e.ValorCondominio)
                .HasPrecision(10, 2)
                .HasColumnName("valor_condominio");
            entity.Property(e => e.ValorImovel)
                .HasPrecision(10, 2)
                .HasColumnName("valor_imovel");
            entity.Property(e => e.ValorRenda)
                .HasPrecision(10, 2)
                .HasColumnName("valor_renda");

            entity.HasOne(d => d.AtivoUu).WithMany(p => p.ImovelArrendados)
                .HasForeignKey(d => d.AtivoUuid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("imovel_arrendado_ativo_uuid_fkey");
        });

        modelBuilder.Entity<Relatorio>(entity =>
        {
            entity.HasKey(e => e.RelatorioUuid).HasName("relatorio_pkey");

            entity.ToTable("relatorio");

            entity.Property(e => e.RelatorioUuid)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("relatorio_uuid");
            entity.Property(e => e.DataFim).HasColumnName("data_fim");
            entity.Property(e => e.DataInicio).HasColumnName("data_inicio");
            entity.Property(e => e.UserUuid).HasColumnName("user_uuid");

            entity.HasOne(d => d.UserUu).WithMany(p => p.Relatorios)
                .HasForeignKey(d => d.UserUuid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("relatorio_user_uuid_fkey");

            entity.HasMany(d => d.AtivoUus).WithMany(p => p.RelatorioUus)
                .UsingEntity<Dictionary<string, object>>(
                    "RelatorioAtivo",
                    r => r.HasOne<Ativo>().WithMany()
                        .HasForeignKey("AtivoUuid")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("relatorio_ativos_ativo_uuid_fkey"),
                    l => l.HasOne<Relatorio>().WithMany()
                        .HasForeignKey("RelatorioUuid")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("relatorio_ativos_relatorio_uuid_fkey"),
                    j =>
                    {
                        j.HasKey("RelatorioUuid", "AtivoUuid").HasName("relatorio_ativos_pkey");
                        j.ToTable("relatorio_ativos");
                        j.IndexerProperty<Guid>("RelatorioUuid").HasColumnName("relatorio_uuid");
                        j.IndexerProperty<Guid>("AtivoUuid").HasColumnName("ativo_uuid");
                    });
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.UserUuid).HasName("usuario_pkey");

            entity.ToTable("usuario");

            entity.Property(e => e.UserUuid)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("user_uuid");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.Senha)
                .HasMaxLength(255)
                .HasColumnName("senha");
            entity.Property(e => e.TipoPerfil)
                .HasMaxLength(50)
                .HasColumnName("tipo_perfil");
            entity.Property(e => e.Username)
                .HasMaxLength(255)
                .HasColumnName("username");
        });
        
        OnModelCreatingPartial(modelBuilder);
    }
    public bool CanConnect()
    {
        try
        {
            return this.Database.CanConnect();
        }
        catch (Exception)
        {
            return false;
        }
    }
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
