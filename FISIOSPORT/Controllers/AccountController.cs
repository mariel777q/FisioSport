using FISIOSPORT.Data;
using FISIOSPORT.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FISIOSPORT.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var fisio = await _context.Fisioterapeutas
                .FirstOrDefaultAsync(f => f.Correo.ToLower() == model.Correo.ToLower());

            if (fisio != null && BCrypt.Net.BCrypt.Verify(model.Password, fisio.PasswordHash))
            {
                await IniciarSesionAsync(fisio.Id, fisio.Nombre, "Fisioterapeuta");
                return RedirectToAction("Dashboard", "Fisioterapeuta");
            }

            var paciente = await _context.Pacientes
                .FirstOrDefaultAsync(p => p.Correo.ToLower() == model.Correo.ToLower());

            if (paciente != null && BCrypt.Net.BCrypt.Verify(model.Password, paciente.PasswordHash))
            {
                await IniciarSesionAsync(paciente.Id, paciente.Nombre, "Paciente");
                return RedirectToAction("Dashboard", "Paciente");
            }

            model.MensajeError = "Correo o contraseña incorrectos.";
            return View(model);
        }

        private async Task IniciarSesionAsync(int id, string nombre, string rol)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, id.ToString()),
                new Claim(ClaimTypes.Name, nombre),
                new Claim(ClaimTypes.Role, rol)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}