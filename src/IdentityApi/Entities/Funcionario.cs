using System.ComponentModel.DataAnnotations;

namespace IdentityService.Entities
{
    public class Funcionario
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(100)]
        public required string Nome { get; set; }

        [Required]
        [MaxLength(50)]
        public required string Cargo { get; set; }

        [Required]
        [MaxLength(100)]
        public required string Email { get; set; }

        [Required]
        public required string SenhaHash { get; set; }
    }

}
