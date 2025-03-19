using System;
using System.Collections.Generic;

namespace AtivosFinanceiros.Models;

public partial class Usuario
{
    public Guid UserUuid { get; set; }

    public string Email { get; set; } = null!;

    public string Senha { get; set; } = null!;

    public string Username { get; set; } = null!;

    public string? TipoPerfil { get; set; }

    public virtual ICollection<Ativo> Ativos { get; set; } = new List<Ativo>();

    public virtual ICollection<Relatorio> Relatorios { get; set; } = new List<Relatorio>();
}
