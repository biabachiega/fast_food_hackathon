using System.ComponentModel.DataAnnotations;

namespace IdentityService.Entities.Dto
{

    public class AtualizarUsuarioDto
    {
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres.")]
        public string? Nome { get; set; }

        [Phone(ErrorMessage = "Telefone inválido.")]
        public string? Telefone { get; set; }

        [StringLength(200, ErrorMessage = "O endereço deve ter no máximo 200 caracteres.")]
        public string? Endereco { get; set; }

        [RegularExpression(@"^\d{11}$", ErrorMessage = "CPF deve conter exatamente 11 dígitos numéricos.")]
        public string? Cpf { get; set; }

        [EmailAddress(ErrorMessage = "E-mail inválido.")]
        public string? Email { get; set; }

        [StringLength(100, MinimumLength = 6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres.")]
        public string? Senha { get; set; }
    }

}
