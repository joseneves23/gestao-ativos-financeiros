using System;
using System.Collections.Generic;

namespace AtivosFinanceiros.Models;

public partial class DepositoPrazo
{
    public Guid DepositoUuid { get; set; }

    public Guid AtivoUuid { get; set; }

    public string Banco { get; set; } = null!;

    public string NumeroConta { get; set; } = null!;

    public string? Titulares { get; set; }

    public decimal? TaxaAnual { get; set; }

    public decimal ValorInicial { get; set; }

    public virtual Ativo AtivoUu { get; set; } = null!;
}
