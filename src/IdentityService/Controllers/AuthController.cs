using IdentityService.Data;
using IdentityService.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace IdentityService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {

        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher<Funcionario> _passwordHasher;
        private readonly PasswordHasher<Usuario> _usuarioPasswordHasher;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
            _usuarioPasswordHasher = new PasswordHasher<Usuario>();
            _passwordHasher = new PasswordHasher<Funcionario>();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
        {
            var funcionario = await _context.Funcionarios
                .FirstOrDefaultAsync(f => f.Email == dto.EmailOuCpf);

            if (funcionario != null)
            {
                var result = _passwordHasher.VerifyHashedPassword(funcionario, funcionario.SenhaHash, dto.Senha);

                if (result == PasswordVerificationResult.Success)
                {
                    return Ok(new LoginResponseDto
                    {
                        Tipo = "Funcionario",
                        Id = funcionario.Id,
                        Nome = funcionario.Nome
                    });
                }
            }

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == dto.EmailOuCpf || u.Cpf == dto.EmailOuCpf);

            if (usuario != null)
            {
                var result = _usuarioPasswordHasher.VerifyHashedPassword(usuario, usuario.SenhaHash, dto.Senha);

                if (result == PasswordVerificationResult.Success)
                {
                    return Ok(new LoginResponseDto
                    {
                        Tipo = "Usuario",
                        Id = usuario.Id,
                        Nome = usuario.Nome
                    });
                }
            }

            return Unauthorized(new { message = "Credenciais inválidas." });
        }
    }
}
