using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace UserManagement.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        [DisplayName("Email Address")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required]
        [DisplayName("Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }


        [DisplayName("Remember me?")]
        public bool RememberMe { get; set; }
    }
}
