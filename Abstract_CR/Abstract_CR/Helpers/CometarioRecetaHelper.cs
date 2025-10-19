using Abstract_CR.Data;
using Abstract_CR.Models;

namespace Abstract_CR.Helpers
{
    public class CometarioRecetaHelper
    {
        private readonly ApplicationDbContext _context;

        public CometarioRecetaHelper(ApplicationDbContext context)
        {
            _context = context;
        }

        public bool AgregarComentario(ComentarioReceta comentarioReceta)
        {
            _context.ComentarioRecetas.Add(comentarioReceta);
            var result = _context.SaveChanges();
            return result > 0;
        }

        public IEnumerable<ComentarioReceta> ObtenerComentariosPorReceta(int recetaId)
        {
            return _context.ComentarioRecetas
                           .Where(c => c.RecetaID == recetaId)
                           .OrderByDescending(c => c.FechaComentario)
                           .ToList();
        }

        public bool EliminarComentario(int comentarioId)
        {
            var comentario = _context.ComentarioRecetas.Find(comentarioId);
            if (comentario != null)
            {
                _context.ComentarioRecetas.Remove(comentario);
                var result = _context.SaveChanges();
                return result > 0;
            }
            return false;
        }
    }
}
