using System;
using System.Collections.Generic;

namespace AtivosFinanceiros.Models;

public partial class ImovelArrendado
{
    public Guid ImovelUuid { get; set; }

    public Guid AtivoUuid { get; set; }

    public string Designacao { get; set; } = null!;

    public string Localizacao { get; set; } = null!;

    public decimal ValorImovel { get; set; }

    public decimal ValorRenda { get; set; }

    public decimal ValorCondominio { get; set; }

    public decimal DespesasAnuais { get; set; }

    public virtual Ativo AtivoUu { get; set; } = null!;
}
