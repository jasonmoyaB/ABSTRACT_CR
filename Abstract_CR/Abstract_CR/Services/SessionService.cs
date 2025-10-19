using Microsoft.AspNetCore.Http;

namespace Abstract_CR.Services
{
    public class SessionService : ISessionService
    {
        private const string UsuarioIdKey = "UsuarioID";
        private const string NombreUsuarioKey = "NombreUsuario";
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SessionService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? ObtenerNombreUsuario()
        {
            return _httpContextAccessor.HttpContext?.Session.GetString(NombreUsuarioKey);
        }

        public int? ObtenerUsuarioId()
        {
            return _httpContextAccessor.HttpContext?.Session.GetInt32(UsuarioIdKey);
        }
    }
}
