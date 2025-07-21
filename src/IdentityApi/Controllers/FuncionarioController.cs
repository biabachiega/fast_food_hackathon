using IdentityService.Data;
using IdentityService.Entities;
using IdentityService.Entities.Dto;
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var funcionarios = await _context.Funcionarios.ToListAsync();
            return Ok(funcionarios);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Create([FromBody] CreateFuncionarioDto dto)
        {

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var emailExistente = await _context.Funcionarios
                .AnyAsync(f => f.Email.ToLower() == dto.Email.ToLower());

            if (emailExistente)
                return Conflict(new { message = "J� existe um funcion�rio com este e-mail." });

            var funcionario = new Funcionario
            {
                Nome = dto.Nome,
                Cargo = dto.Cargo,
                Email = dto.Email,
                SenhaHash = _passwordHasher.HashPassword(new Funcionario { Nome = dto.Nome, Cargo = dto.Cargo, Email = dto.Email, SenhaHash = "" }, dto.Senha)
            };

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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Update(Guid id, [FromBody] AtualizarFuncionarioDto dto)
        {

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var funcionario = await _context.Funcionarios.FindAsync(id);

            if (funcionario == null)
                return NotFound();

            if (!string.IsNullOrWhiteSpace(dto.Nome))
                funcionario.Nome = dto.Nome;

            if (!string.IsNullOrWhiteSpace(dto.Cargo))
                funcionario.Cargo = dto.Cargo;

            if (!string.IsNullOrWhiteSpace(dto.Email)) {
                var emailUsado = await _context.Funcionarios
                    .AnyAsync(f => f.Email.ToLower() == dto.Email.ToLower() && f.Id != id);

                if (emailUsado)
                    return Conflict(new { message = "J� existe outro funcion�rio com este e-mail." });

                funcionario.Email = dto.Email;
            }

            if (!string.IsNullOrWhiteSpace(dto.Senha))
                funcionario.SenhaHash = _passwordHasher.HashPassword(funcionario, dto.Senha);

            await _context.SaveChangesAsync();
            return Ok(new { message = "Funcionário atualizado com sucesso.", funcionarioId = id });
        }

        [HttpDelete("deleteById/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteFuncionario(Guid id)
        {
            var funcionario = await _context.Funcionarios.FindAsync(id);

            if (funcionario == null)
                return NotFound();

            _context.Funcionarios.Remove(funcionario);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Funcionário removido com sucesso.", funcionarioId = id });
        }


    }
}
