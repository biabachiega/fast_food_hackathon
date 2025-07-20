using System.ComponentModel.DataAnnotations;

namespace IdentityService.Entities.Dto
{
    public class CreateUsuarioDto
    {
        [Required]
        [StringLength(100)]
        public required string Nome { get; set; }

        [Required]
        [Phone(ErrorMessage = "Telefone inválido")]
        public required string Telefone { get; set; }

        [Required]
        [StringLength(200)]
        public required string Endereco { get; set; }

        [Required]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "CPF deve conter 11 dígitos numéricos")]
        public required string Cpf { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "E-mail inválido")]
        public required string Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres")]
        public required string Senha { get; set; }

    }
}
