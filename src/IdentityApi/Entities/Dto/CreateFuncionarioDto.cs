using System.ComponentModel.DataAnnotations;

namespace IdentityService.Entities.Dto
{

    public class CreateFuncionarioDto
    {
        [Required]
        [StringLength(100)]
        public required string Nome { get; set; }

        [Required]
        [StringLength(50)]
        public required string Cargo { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "E-mail inválido")]
        public required string Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres")]
        public required string Senha { get; set; }
    }

}
