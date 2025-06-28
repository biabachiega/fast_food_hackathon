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
    }
}
