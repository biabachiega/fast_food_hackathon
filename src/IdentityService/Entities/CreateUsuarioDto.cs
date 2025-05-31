using System.ComponentModel.DataAnnotations;

namespace IdentityService.Entities
{
    public class CreateUsuarioDto
    {
        public string Nome { get; set; }
        public string Telefone { get; set; }
        public string Endereco { get; set; }
        public string Cpf { get; set; }
        public string Email { get; set; }
        public string Senha { get; set; }
    }
}
