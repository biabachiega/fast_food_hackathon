using IdentityService.Data;
using IdentityService.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FuncionarioController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FuncionarioController> _logger;
        private readonly PasswordHasher<Funcionario> _passwordHasher;

        public FuncionarioController(ApplicationDbContext context, ILogger<FuncionarioController> logger)
        {
            _context = context;
            _logger = logger;
            _passwordHasher = new PasswordHasher<Funcionario>();
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var funcionarios = await _context.Funcionarios.ToListAsync();
            return Ok(funcionarios);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateFuncionarioDto dto)
        {

            var funcionario = new Funcionario
            {
                Nome = dto.Nome,
                Cargo = dto.Cargo,
                Email = dto.Email
            };

            funcionario.SenhaHash = _passwordHasher.HashPassword(funcionario, dto.Senha);

            _context.Funcionarios.Add(funcionario);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = funcionario.Id }, new
            {
                funcionario.Id,
                funcionario.Nome,
                funcionario.Cargo,
                funcionario.Email
            });
        }

        [HttpGet("getById/{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var funcionario = await _context.Funcionarios.FindAsync(id);

            if (funcionario == null)
                return NotFound();

            return Ok(new
            {
                funcionario.Id,
                funcionario.Nome,
                funcionario.Cargo,
                funcionario.Email
            });
        }

        [HttpPut("updateById/{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] AtualizarFuncionarioDto dto)
        {
            var funcionario = await _context.Funcionarios.FindAsync(id);

            if (funcionario == null)
                return NotFound();

            if (!string.IsNullOrWhiteSpace(dto.Nome))
                funcionario.Nome = dto.Nome;

            if (!string.IsNullOrWhiteSpace(dto.Cargo))
                funcionario.Cargo = dto.Cargo;

            if (!string.IsNullOrWhiteSpace(dto.Email))
                funcionario.Email = dto.Email;

            if (!string.IsNullOrWhiteSpace(dto.Senha))
                funcionario.SenhaHash = _passwordHasher.HashPassword(funcionario, dto.Senha);

            await _context.SaveChangesAsync();
            return NoContent();
        }

    }
}
