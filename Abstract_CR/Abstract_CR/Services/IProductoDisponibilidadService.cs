using System.Collections.Generic;

namespace Abstract_CR.Services
{
    public interface IProductoDisponibilidadService
    {
        IDictionary<string, ProductoDisponibilidadResult> VerificarDisponibilidad(IEnumerable<string> nombresProductos);
    }

    public class ProductoDisponibilidadResult
    {
        public bool Disponible { get; set; }
        public List<string> Sugerencias { get; set; } = new();
    }
}
