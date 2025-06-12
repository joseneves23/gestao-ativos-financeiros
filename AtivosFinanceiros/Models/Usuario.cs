using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AtivosFinanceiros.Models;

public partial class Usuario
{
    public Guid UserUuid { get; set; }

    [Required(ErrorMessage = "O email é obrigatório")]
    [EmailAddress(ErrorMessage = "O email informado não é válido")]

    public string Email { get; set; } = null!;

    public string Senha { get; set; } = null!;

    public string Username { get; set; } = null!;

    public string? TipoPerfil { get; set; }

    public virtual ICollection<Ativo> Ativos { get; set; } = new List<Ativo>();

    public virtual ICollection<Relatorio> Relatorios { get; set; } = new List<Relatorio>();
}
