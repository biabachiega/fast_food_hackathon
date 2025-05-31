using Microsoft.AspNetCore.Identity;

namespace IdentityService.Entities
{

    public class ApplicationUser : IdentityUser
    {
        public UserType UserType { get; set; } 
    }

    public enum UserType
    {
        Cliente,
        Funcionario
    }
}
