using System.ComponentModel.DataAnnotations;

namespace IdentityService.Entities
{
    public class Usuario
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(100)]
        public required string Nome { get; set; }

        [Required]
        [Phone]
        public required string Telefone { get; set; }

        [Required]
        [MaxLength(200)]
        public required string Endereco { get; set; }

        [Required]
        [StringLength(11)]
        public required string Cpf { get; set; }

        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        public required string SenhaHash { get; set; }
    }
}

