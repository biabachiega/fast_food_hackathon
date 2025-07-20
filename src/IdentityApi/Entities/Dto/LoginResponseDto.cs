namespace IdentityService.Entities.Dto
{
    public class LoginResponseDto
    {
        public required string Tipo { get; set; } 
        public Guid Id { get; set; }
        public required string Nome { get; set; }
    }

}
