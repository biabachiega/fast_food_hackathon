namespace OrderApi.Entities
{
    public class ItemPedido
    {
        public Guid Id { get; set; }
        public Guid PedidoId { get; set; }
        public string Produto { get; set; }
        public int Quantidade { get; set; }
        public decimal PrecoUnitario { get; set; }
    }
}
