using System;
using System.Collections.Generic;

namespace Firmeza.Domain.Entities;

public partial class Venta
{
    public Guid Id { get; set; }

    public Guid ClienteId { get; set; }

    public DateTime Fecha { get; set; }

    public decimal Total { get; set; }

    public string Estado { get; set; } = null!;

    public virtual Cliente Cliente { get; set; } = null!;

    public virtual ICollection<Detalle> Detalles { get; set; } = new List<Detalle>();
}
