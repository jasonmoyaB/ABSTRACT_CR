using Abstract_CR.Helpers;
using Abstract_CR.Services;
using Microsoft.AspNetCore.Mvc;
using System.Numerics;

namespace Abstract_CR.Controllers
{
    public class MenuController : Controller
    {
        private readonly IEmailService _emailService;
        private readonly UserHelper _userHelper;

        public MenuController(IEmailService emailService, UserHelper userHelper)
        {
            _emailService = emailService;
            _userHelper = userHelper;
        }

        [HttpPost]
        public async Task<IActionResult> AsignarMenu(int MenuID, int UsuarioID)
        {
            //Agregar lógica para asignar menú en base de datos aquí
            var menuAssignado = true;
            if (menuAssignado)
            {
                var usuarioAsignado = _userHelper.GetUsuarioPorId(UsuarioID);
                var subject = $"Se le ha asignado un nuevo menú";
                var body = $@"
                            <html>
                                <body style='font-family:Arial,Helvetica,sans-serif; line-height:1.5;'>
                                <h2>El chef le ha asignado un nuevo menú</h2>
                                <p><strong>Menu:</strong> DESCRIPCION DE MENU</p>
                                <hr/>
                                <p style='font-size:12px;color:#666'>Si ya revisate el menú, puedes ignorar este mensaje.</p>
                                </body>
                            </html>";
                try { await _emailService.SendEmailAsync(usuarioAsignado.CorreoElectronico, subject, body); } catch { }
            }

            return View();
        }
    }
}
