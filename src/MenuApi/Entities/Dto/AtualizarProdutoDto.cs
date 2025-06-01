namespace MenuApi.Entities.Dto
{
    public class AtualizarProdutoDto
    {
        public string? Nome { get; set; }
        public string? Descricao { get; set; }
        public decimal? Preco { get; set; }
        public string? Tipo { get; set; } 
        public bool? Disponivel { get; set; }
    }

}
