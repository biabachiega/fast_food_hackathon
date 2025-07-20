using System.ComponentModel.DataAnnotations;

namespace MenuApi.Entities.Dto
{
    public class CreateProdutoDto
    {
        [Required]
        [MaxLength(100)]
        public required string Nome { get; set; }

        [MaxLength(500)]
        public required string Descricao { get; set; }

        [Required]
        [Range(0.01, 1000)]
        public decimal Preco { get; set; }

        [Required]
        public required string Tipo { get; set; }

        public bool Disponivel { get; set; } = true;

        public int Quantidade { get; set; }
    }
}
