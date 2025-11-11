using Abstract_CR.Data;
using Abstract_CR.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Abstract_CR.Helpers
{
    public class InteraccionHelper
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<InteraccionHelper> _logger;

        public InteraccionHelper(ApplicationDbContext context, ILogger<InteraccionHelper> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<UsuarioInteraccionResumen>> ObtenerUsuariosAsync()
        {
            return await _context.Usuarios
                .AsNoTracking()
                .OrderBy(u => u.Nombre)
                .Select(u => new UsuarioInteraccionResumen
                {
                    UsuarioId = u.UsuarioID,
                    NombreCompleto = u.Nombre + " " + u.Apellido,
                    CorreoElectronico = u.CorreoElectronico,
                    PuntosTotales = u.PuntosTotales,
                    UltimaActividad = u.Mensajes
                        .OrderByDescending(m => m.FechaEnvio)
                        .Select(m => (DateTime?)m.FechaEnvio)
                        .FirstOrDefault()
                })
                .ToListAsync();
        }

        public async Task<(Usuario? usuario, List<MensajeInteraccion> mensajes, List<PuntosUsuario> historial)> ObtenerDetalleUsuarioAsync(int usuarioId)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.UsuarioID == usuarioId);

            if (usuario == null)
            {
                return (null, new List<MensajeInteraccion>(), new List<PuntosUsuario>());
            }

            var mensajes = await _context.MensajesInteraccion
                .Where(m => m.UsuarioId == usuarioId)
                .OrderByDescending(m => m.FechaEnvio)
                .Include(m => m.Remitente)
                .AsNoTracking()
                .ToListAsync();

            var historial = await _context.PuntosUsuarios
                .Where(p => p.UsuarioId == usuarioId)
                .OrderByDescending(p => p.FechaAsignacion)
                .Include(p => p.AsignadoPor)
                .AsNoTracking()
                .ToListAsync();

            return (usuario, mensajes, historial);
        }

        public async Task<(bool success, string? error)> RegistrarMensajeAsync(int usuarioId, string contenido, bool enviadoPorChef, TipoMensajeInteraccion tipo, int? remitenteId = null)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.UsuarioID == usuarioId);
            if (usuario == null)
            {
                return (false, "Usuario no encontrado");
            }

            var mensaje = new MensajeInteraccion
            {
                UsuarioId = usuarioId,
                RemitenteId = remitenteId,
                EnviadoPorChef = enviadoPorChef,
                Contenido = contenido,
                Tipo = tipo,
                FechaEnvio = DateTime.Now,
                Leido = enviadoPorChef // si lo envía el chef se marca como leído por defecto para admin
            };

            _context.MensajesInteraccion.Add(mensaje);

            try
            {
                await _context.SaveChangesAsync();
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar mensaje");
                return (false, "No se pudo guardar el mensaje");
            }
        }

        public async Task<(bool success, string? error)> AsignarPuntosAsync(int usuarioId, int puntos, string? motivo, int? adminId = null)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.UsuarioID == usuarioId);
            if (usuario == null)
            {
                return (false, "Usuario no encontrado");
            }

            var registro = new PuntosUsuario
            {
                UsuarioId = usuarioId,
                Puntos = puntos,
                Motivo = motivo,
                FechaAsignacion = DateTime.Now,
                AsignadoPorId = adminId
            };

            usuario.PuntosTotales += puntos;

            _context.PuntosUsuarios.Add(registro);
            _context.Usuarios.Update(usuario);

            try
            {
                await _context.SaveChangesAsync();
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al asignar puntos");
                return (false, "No se pudo asignar los puntos");
            }
        }

        public async Task MarcarMensajesComoLeidosAsync(int usuarioId, bool paraChef)
        {
            var mensajes = await _context.MensajesInteraccion
                .Where(m => m.UsuarioId == usuarioId && m.EnviadoPorChef != paraChef && !m.Leido)
                .ToListAsync();

            if (!mensajes.Any())
            {
                return;
            }

            foreach (var mensaje in mensajes)
            {
                mensaje.Leido = true;
            }

            await _context.SaveChangesAsync();
        }
    }
}

