using System;
using System.Collections.Generic;

namespace AtivosFinanceiros.Models;

public partial class Relatorio
{
    public Guid RelatorioUuid { get; set; }

    public Guid UserUuid { get; set; }

    public DateOnly DataInicio { get; set; }

    public DateOnly? DataFim { get; set; }

    public string TipoRelatorio { get; set; } = null!;

    public virtual Usuario UserUu { get; set; } = null!;

    public virtual ICollection<Ativo> AtivoUus { get; set; } = new List<Ativo>();
}
