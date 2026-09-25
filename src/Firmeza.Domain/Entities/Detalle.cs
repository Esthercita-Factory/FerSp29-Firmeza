using System;
using System.Collections.Generic;

namespace Firmeza.Domain.Entities;

public partial class Detalle
{
    public Guid Id { get; set; }

    public Guid VentaId { get; set; }

    public Guid ProductoId { get; set; }

    public int Cantidad { get; set; }

    public decimal PrecioUnitario { get; set; }

    public decimal Subtotal { get; set; }

    public virtual Producto Producto { get; set; } = null!;

    public virtual Venta Venta { get; set; } = null!;
}
