namespace OrderApi.Entities.Dto
{
    public class PedidoDto
    {
        public List<ItemPedidoDto> Itens { get; set; }
        public FormaEntrega FormaEntrega { get; set; }
    }
}
