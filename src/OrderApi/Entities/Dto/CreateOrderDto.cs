namespace OrderApi.Entities.Dto
{
    public class CreateOrderDto
    {
        public Guid ClienteId { get; set; }
        public List<OrderItemDto> Itens { get; set; }
        public string FormaEntrega { get; set; } // Ex: "balcao", "drive-thru", "delivery"
    }

    public class OrderItemDto
    {
        public Guid ProdutoId { get; set; }
        public int Quantidade { get; set; }
    }
}
