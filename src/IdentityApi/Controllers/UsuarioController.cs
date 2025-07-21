using IdentityService.Data;
using IdentityService.Entities;
using IdentityService.Entities.Dto;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuarioController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher<Usuario> _passwordHasher;

        public UsuarioController(ApplicationDbContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<Usuario>();
        }


        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var usuarios = await _context.Usuarios.ToListAsync();
            return Ok(usuarios);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateUsuarioDto dto)
        {

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var usuario = new Usuario
            {
                Nome = dto.Nome,
                Email = dto.Email,
                Cpf = dto.Cpf,
                Endereco = dto.Endereco,
                Telefone = dto.Telefone,
                SenhaHash = _passwordHasher.HashPassword(new Usuario { Nome = dto.Nome, Email = dto.Email, Cpf = dto.Cpf, Endereco = dto.Endereco, Telefone = dto.Telefone, SenhaHash = "" }, dto.Senha)
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = usuario.Id }, new
            {
                usuario.Id,
                usuario.Nome,
                usuario.Cpf,
                usuario.Email,
                usuario.Endereco,
                usuario.Telefone
            });
        }

        [HttpGet("getById/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
                return NotFound();

            return Ok(new
            {
                usuario.Id,
                usuario.Nome,
                usuario.Cpf,
                usuario.Email,
                usuario.Endereco,
                usuario.Telefone
            });
        }

        [HttpPut("updateById/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid id, [FromBody] AtualizarUsuarioDto dto)
        {
            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
                return NotFound();

            if (!string.IsNullOrWhiteSpace(dto.Nome))
                usuario.Nome = dto.Nome;

            if (!string.IsNullOrWhiteSpace(dto.Telefone))
                usuario.Telefone = dto.Telefone;

            if (!string.IsNullOrWhiteSpace(dto.Endereco))
                usuario.Endereco = dto.Endereco;

            if (!string.IsNullOrWhiteSpace(dto.Cpf))
                usuario.Cpf = dto.Cpf;

            if (!string.IsNullOrWhiteSpace(dto.Email))
                usuario.Email = dto.Email;

            if (!string.IsNullOrWhiteSpace(dto.Senha))
                usuario.SenhaHash = _passwordHasher.HashPassword(usuario, dto.Senha);

            await _context.SaveChangesAsync();
            return Ok(new { message = "Usuário atualizado com sucesso.", usuarioId = id });
        }

        [HttpDelete("deleteById/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteUsuario(Guid id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
                return NotFound();

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Usuário removido com sucesso.", usuarioId = id });
        }

    }
}
