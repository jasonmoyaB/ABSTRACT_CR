using Abstract_CR.Data;
using Abstract_CR.Models;
using Microsoft.EntityFrameworkCore;

namespace Abstract_CR.Helpers
{
    public class EbooksHelper
    {
        private readonly ApplicationDbContext _context;

        public EbooksHelper(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<EbookEdicion> GetEbooks()
        {
            var ediciones = _context.EbookEdicion
                .Where(e => e.Estado)
                .OrderBy(e => e.Escenario)
                .AsNoTracking()
                .ToList();

            return ediciones;
        }

        public EbookEdicion? GetEbookById(int id)
        {
            return _context.EbookEdicion
                .AsNoTracking()
                .FirstOrDefault(e => e.EbookEdicionID == id && e.Estado);
        }
    }
}
