namespace OrderApi.Entities
{
    public class Pedido
    {
        public Guid Id { get; set; }
        public Guid Cliente { get; set; }
        public required List<ItemPedido> Itens { get; set; }
        public decimal Total { get; set; }
        public StatusPedido Status { get; set; }
        public required string Justificativa { get; set; } 
        public FormaEntrega FormaEntrega { get; set; }
        public DateTime DataCriacao { get; set; }
    }
}
