using OrderApi.Entities.Dto;

namespace OrderApi.Services
{
    public class MenuService
    {
        private readonly HttpClient _httpClient;

        public MenuService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ProdutoResponseDto?> GetProdutoByIdAsync(Guid produtoId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"Produtos/{produtoId}");
                if (!response.IsSuccessStatusCode)
                    return null;

                return await response.Content.ReadFromJsonAsync<ProdutoResponseDto>();

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task<bool> AtualizarEstoqueAsync(Guid produtoId, int novaQuantidade)
        {
            try
            {
                // Se a quantidade for maior que 0, marcar como disponível
                var updateDto = new { 
                    Quantidade = novaQuantidade,
                    Disponivel = novaQuantidade > 0
                };
                var response = await _httpClient.PutAsJsonAsync($"Produtos/{produtoId}", updateDto);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao atualizar estoque do produto {produtoId}: {ex.Message}", ex);
            }
        }
    }
}
