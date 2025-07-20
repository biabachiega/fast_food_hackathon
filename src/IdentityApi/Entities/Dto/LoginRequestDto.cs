namespace IdentityService.Entities.Dto
{
    public class LoginRequestDto
    {
        public required string EmailOuCpf { get; set; }
        public required string Senha { get; set; }
    }

}
