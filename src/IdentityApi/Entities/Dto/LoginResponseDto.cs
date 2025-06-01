namespace IdentityService.Entities.Dto
{
    public class LoginResponseDto
    {
        public string Tipo { get; set; } // "Funcionario" ou "Usuario"
        public Guid Id { get; set; }
        public string Nome { get; set; }
    }

}
