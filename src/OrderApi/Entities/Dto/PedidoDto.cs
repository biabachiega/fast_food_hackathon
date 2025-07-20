namespace OrderApi.Entities.Dto
{
    public class PedidoDto
    {
        public required List<ItemPedidoDto> Itens { get; set; }
        public FormaEntrega FormaEntrega { get; set; }
    }
}
