namespace OrderApi.Entities.Dto
{
    public class ItemPedidoDto
    {
        public string Produto { get; set; }
        public int Quantidade { get; set; }
        public decimal PrecoUnitario { get; set; }
    }
}
