using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace UserManagement.Models.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [DisplayName("Email Address")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
    }
}
