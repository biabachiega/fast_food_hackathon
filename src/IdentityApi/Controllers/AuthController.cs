using IdentityService.Data;
using IdentityService.Entities;
using IdentityService.Entities.Dto;
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
        private static readonly Dictionary<string, LoginAttemptTracker> _loginAttempts = new();
        private const int MaxAttempts = 5;
        private static readonly TimeSpan BlockTime = TimeSpan.FromMinutes(5);
        public AuthController(ApplicationDbContext context)
        {
            _context = context;
            _usuarioPasswordHasher = new PasswordHasher<Usuario>();
            _passwordHasher = new PasswordHasher<Funcionario>();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
        {
            var key = dto.EmailOuCpf.ToLower();
            if (_loginAttempts.TryGetValue(key, out var tracker))
            {
                if (tracker.BlockUntil.HasValue && tracker.BlockUntil > DateTime.UtcNow)
                    return BadRequest(new { message = $"Credenciais inválidas ou tentativa bloqueada. Tente novamente mais tarde." });

                if ((DateTime.UtcNow - tracker.LastAttempt).TotalMinutes > 10)
                    _loginAttempts.Remove(key);
            }

            var funcionario = await _context.Funcionarios
                .FirstOrDefaultAsync(f => f.Email == dto.EmailOuCpf);

            if (funcionario != null)
            {
                var result = _passwordHasher.VerifyHashedPassword(funcionario, funcionario.SenhaHash, dto.Senha);
                if (result == PasswordVerificationResult.Success)
                {
                    _loginAttempts.Remove(key);
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
                    _loginAttempts.Remove(key); 
                    return Ok(new LoginResponseDto
                    {
                        Tipo = "Usuario",
                        Id = usuario.Id,
                        Nome = usuario.Nome
                    });
                }
            }


            if (!_loginAttempts.ContainsKey(key))
                _loginAttempts[key] = new LoginAttemptTracker();

            var attempt = _loginAttempts[key];
            attempt.Attempts++;
            attempt.LastAttempt = DateTime.UtcNow;

            if (attempt.Attempts >= MaxAttempts)
                attempt.BlockUntil = DateTime.UtcNow.Add(BlockTime);

            return Unauthorized(new { message = "Credenciais inválidas." });
        }

    }
}
