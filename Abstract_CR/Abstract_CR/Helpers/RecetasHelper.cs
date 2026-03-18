using Abstract_CR.Data;
using Abstract_CR.Models;

namespace Abstract_CR.Helpers
{
    public class RecetasHelper
    {
        private readonly ApplicationDbContext _context;

        public RecetasHelper(ApplicationDbContext context)
        {
            _context = context;
        }

        public Receta GetRecetaPorId(int id)
        {
            var receta = _context.Recetas.FirstOrDefault(r => r.RecetaID == id);

            if (receta == null)
            {
                return new Receta();
            }

            return receta;
        }

        public IEnumerable<Receta> GetRecetas()
        {
            return _context.Recetas.ToList();
        }

        public IEnumerable<RecetasViewModel> GetRecetasViewModel()
        {
            var recetas = from r in _context.Recetas
                          join u in _context.Usuarios on r.ChefID equals u.UsuarioID
                          join ro in _context.Roles on u.RolID equals ro.RolID
                          where ro.NombreRol == "Admin"
                          select new RecetasViewModel
                          {
                              RecetaID = r.RecetaID,
                              Titulo = r.Titulo,
                              Descripcion = r.Descripcion,
                              Instrucciones = r.Instrucciones,
                              EsGratuita = r.EsGratuita,
                              EsParteDeEbook = r.EsParteDeEbook,
                              Chef = u.Nombre
                          };

            return [.. recetas];
        }

        public bool AsignarRecetas(int recetaId, int personaId, string diaSemana)
        {
            _context.RecetasPorUsuario.Add(new RecetaPorUsuario
                                           {
                                               RecetaID = recetaId,
                                               UsuarioID = personaId,
                                               Dia = diaSemana
                                           });
            var result = _context.SaveChanges();
            return result > 0;
        }
    }
}
