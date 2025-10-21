using System.Collections.Generic;
using System.Linq;

namespace Abstract_CR.Services
{
    public class ProductoDisponibilidadService : IProductoDisponibilidadService
    {
        private readonly HashSet<string> _productosDisponibles = new(StringComparer.OrdinalIgnoreCase)
        {
            "Pollo a la plancha",
            "Salmón al horno",
            "Ensalada de quinoa",
            "Tacos de vegetales",
            "Bowl de garbanzos",
            "Lasaña de berenjena",
            "Wrap de pavo",
            "Smoothie verde"
        };

        private readonly Dictionary<string, List<string>> _sugerencias = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Pollo al curry"] = new() { "Pollo a la plancha", "Tacos de vegetales" },
            ["Salmón grillado"] = new() { "Salmón al horno", "Bowl de garbanzos" },
            ["Hamburguesa veggie"] = new() { "Bowl de garbanzos", "Wrap de pavo" },
            ["Pizza integral"] = new() { "Lasaña de berenjena", "Ensalada de quinoa" }
        };

        public IDictionary<string, ProductoDisponibilidadResult> VerificarDisponibilidad(IEnumerable<string> nombresProductos)
        {
            var resultado = new Dictionary<string, ProductoDisponibilidadResult>(StringComparer.OrdinalIgnoreCase);

            foreach (var nombre in nombresProductos.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (_productosDisponibles.Contains(nombre))
                {
                    resultado[nombre] = new ProductoDisponibilidadResult
                    {
                        Disponible = true
                    };
                }
                else
                {
                    var sugerencias = _sugerencias.TryGetValue(nombre, out var opciones)
                        ? opciones
                        : _productosDisponibles.Take(3).ToList();

                    resultado[nombre] = new ProductoDisponibilidadResult
                    {
                        Disponible = false,
                        Sugerencias = sugerencias
                    };
                }
            }

            return resultado;
        }
    }
}
