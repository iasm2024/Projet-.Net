using System.ComponentModel.DataAnnotations;

namespace PanierMVC.Models
{
    public class User
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Le nom d'utilisateur est requis.")]
        public required string login { get; set; }

        [Required(ErrorMessage = "Le mot de passe est requis.")]
        public required string password { get; set; }


        public required string role { get; set; }
    }
}
