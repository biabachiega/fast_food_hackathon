using System.ComponentModel.DataAnnotations;

namespace IdentityService.Entities.Dto
{
    public class AtualizarFuncionarioDto
    {
        [StringLength(100)]
        public string? Nome { get; set; }

        [StringLength(50)]
        public string? Cargo { get; set; }

        [EmailAddress(ErrorMessage = "E-mail inválido")]
        public string? Email { get; set; }

        [StringLength(100, MinimumLength = 6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres")]
        public string? Senha { get; set; }
    }

}
