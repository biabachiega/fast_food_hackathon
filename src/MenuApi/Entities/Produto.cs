using System.ComponentModel.DataAnnotations;

namespace MenuApi.Entities
{
    public class Produto
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(100)]
        public string Nome { get; set; }

        [MaxLength(300)]
        public string Descricao { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Preco { get; set; }

        [Required]
        public TipoProduto Tipo { get; set; }

        public int Quantidade { get; set; }

        [Required]
        public bool Disponivel { get; set; } = true;
    }

    public enum TipoProduto
    {
        Lanche,
        Sobremesa,
        Bebida
    }
}
