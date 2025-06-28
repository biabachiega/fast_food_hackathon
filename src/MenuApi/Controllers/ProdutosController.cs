using MenuApi.Data;
using MenuApi.Entities.Dto;
using MenuApi.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MenuService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProdutosController : ControllerBase
    {
        private readonly MenuDbContext _context;
        private readonly ILogger<ProdutosController> _logger;

        public ProdutosController(MenuDbContext context, ILogger<ProdutosController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var produtos = await _context.Produtos.ToListAsync();
            return Ok(produtos);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProdutoDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!Enum.TryParse<TipoProduto>(dto.Tipo, true, out var tipoProduto))
                return BadRequest("Tipo de produto inválido. Use: Lanche, Bebida ou Sobremesa.");

            if (dto.Quantidade < 1)
                dto.Disponivel = false;

            var produto = new Produto
            {
                Nome = dto.Nome,
                Descricao = dto.Descricao,
                Preco = dto.Preco,
                Tipo = tipoProduto,
                Disponivel = dto.Disponivel,
                Quantidade = dto.Quantidade
            };

            _context.Produtos.Add(produto);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = produto.Id }, new
            {
                produto.Id,
                produto.Nome,
                produto.Preco
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var produto = await _context.Produtos.FindAsync(id);
            if (produto == null)
                return NotFound();

            return Ok(produto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateById(Guid id, [FromBody] AtualizarProdutoDto dto)
        {
            var produto = await _context.Produtos.FindAsync(id);

            if (produto == null)
                return NotFound();

            if (dto.Quantidade.HasValue)
            {
                produto.Quantidade = (int)dto.Quantidade;
                if (dto.Quantidade < 1)
                    dto.Disponivel = false;
            }

            if (!string.IsNullOrWhiteSpace(dto.Nome))
                produto.Nome = dto.Nome;

            if (!string.IsNullOrWhiteSpace(dto.Descricao))
                produto.Descricao = dto.Descricao;


            if (dto.Preco.HasValue)
                produto.Preco = dto.Preco.Value;

            if (!string.IsNullOrWhiteSpace(dto.Tipo))
            {
                if (!Enum.TryParse<TipoProduto>(dto.Tipo, true, out var tipoProduto))
                    return BadRequest("Tipo de produto inválido. Use: Lanche, Bebida ou Sobremesa.");

                produto.Tipo = tipoProduto;
            }

            if (dto.Disponivel.HasValue)
                produto.Disponivel = dto.Disponivel.Value;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteById(Guid id)
        {
            var produto = await _context.Produtos.FindAsync(id);

            if (produto == null)
                return NotFound();

            _context.Produtos.Remove(produto);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("filterByType")]
        public async Task<IActionResult> GetByTipo([FromQuery] string tipo)
        {
            if (string.IsNullOrWhiteSpace(tipo))
            {
                var allProducts = await _context.Produtos.ToListAsync();
                return Ok(allProducts);
            }

            if (!Enum.TryParse<TipoProduto>(tipo, true, out var tipoProduto))
            {
                return BadRequest("Tipo de produto inválido. Use: Lanche, Bebida ou Sobremesa.");
            }

            var produtosFiltrados = await _context.Produtos
                .Where(p => p.Tipo == tipoProduto)
                .ToListAsync();

            return Ok(produtosFiltrados);
        }

    }
}
