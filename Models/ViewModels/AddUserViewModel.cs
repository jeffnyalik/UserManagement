﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace UserManagement.Models.ViewModels
{
    public class AddUserViewModel
    {   
        [Required]
        [EmailAddress]
        [DisplayName("Email Address")]
        public string Email { get; set; }

        [Required]
        [DisplayName("Password")]
        [StringLength(100, ErrorMessage = "The password must be atleast 6 characters long", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [DisplayName("Confirm Password")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        [Required]
        public string Name { get; set; }

        public List<RoleViewModel>Roles { get; set; }
    }
}
