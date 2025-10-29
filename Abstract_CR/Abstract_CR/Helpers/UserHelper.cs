using Abstract_CR.Data;
using Abstract_CR.Models;
using Microsoft.EntityFrameworkCore;

namespace Abstract_CR.Helpers
{
    public class UserHelper
    {
        private readonly ApplicationDbContext _context;
        public UserHelper(ApplicationDbContext context)
        {
            _context = context;
        }

        public Usuario ObtenerUsuarioPorCorreo(string email)
        {
            var usuario = _context.Usuarios
                                  .Include(u => u.Rol)
                                  .FirstOrDefault(u => u.CorreoElectronico == email && u.Activo);

            if (usuario == null)
            {
                return new Usuario();
            }

            return usuario;
        }

        public bool AgregarToken(PassResetTokens entity)
        {
            _context.Tokens.Add(entity);
            var result = _context.SaveChanges();
            return result > 0;
        }

        public PassResetTokens ObtenerToken(string token)
        {
            var tokenFound = _context.Tokens.Where(u => u.Token == token);
            return tokenFound.Any() ? tokenFound.First() : new PassResetTokens();
        }

        public bool EliminarToken(PassResetTokens passResetToken)
        {
            _context.Tokens.Remove(passResetToken);
            var result = _context.SaveChanges();
            return result > 0;
        }
    }
}
