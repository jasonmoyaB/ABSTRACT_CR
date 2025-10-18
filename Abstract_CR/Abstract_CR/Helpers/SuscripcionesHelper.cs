using Abstract_CR.Data;
using Abstract_CR.Models;

namespace Abstract_CR.Helpers
{
    public class SuscripcionesHelper
    {
        private readonly ApplicationDbContext _context;

        public SuscripcionesHelper(ApplicationDbContext context)
        {
            _context = context;
        }

        public Suscripcion GetSuscripcion(int usuarioId)
        {
            var suscripcion = _context.Suscripciones
                .FirstOrDefault(s => s.UsuarioID == usuarioId);

            return suscripcion ?? new Suscripcion();
        }
    }
}
