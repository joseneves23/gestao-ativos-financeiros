using System;
using System.Collections.Generic;

namespace AtivosFinanceiros.Models;

public partial class FundoInvestimento
{
    public Guid FundoUuid { get; set; }

    public Guid AtivoUuid { get; set; }

    public decimal? JurosPadrao { get; set; }

    public decimal? JurosMensal { get; set; }

    public decimal MonteInvestido { get; set; }

    public virtual Ativo AtivoUu { get; set; } = null!;
}
