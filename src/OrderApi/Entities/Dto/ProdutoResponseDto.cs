namespace OrderApi.Entities.Dto
{
    public class ProdutoResponseDto
    {
        public Guid Id { get; set; }
        public string Nome { get; set; }
        public decimal Preco { get; set; }
        public int Quantidade { get; set; }
        public bool Disponivel { get; set; }
    }
}
