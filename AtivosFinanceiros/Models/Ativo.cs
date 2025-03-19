using System;
using System.Collections.Generic;

namespace AtivosFinanceiros.Models;

public partial class Ativo
{
    public Guid AtivoUuid { get; set; }

    public Guid UserUuid { get; set; }

    public string Nome { get; set; } = null!;

    public DateOnly DataInicio { get; set; }

    public int DuracaoMeses { get; set; }

    public decimal ValorInicial { get; set; }

    public decimal? ImpostoPerc { get; set; }

    public decimal? LucroTotal { get; set; }

    public string? TipoAtivo { get; set; }

    public virtual ICollection<DepositoPrazo> DepositoPrazos { get; set; } = new List<DepositoPrazo>();

    public virtual ICollection<FundoInvestimento> FundoInvestimentos { get; set; } = new List<FundoInvestimento>();

    public virtual ICollection<ImovelArrendado> ImovelArrendados { get; set; } = new List<ImovelArrendado>();

    public virtual Usuario UserUu { get; set; } = null!;

    public virtual ICollection<Relatorio> RelatorioUus { get; set; } = new List<Relatorio>();
}
