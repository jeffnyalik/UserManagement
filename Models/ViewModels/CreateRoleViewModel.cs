

using System.ComponentModel.DataAnnotations;

namespace UserManagement.Models.ViewModels
{
    public class CreateRoleViewModel
    {
        [Required]
        public string RoleName { get; set; }    
    }
}
