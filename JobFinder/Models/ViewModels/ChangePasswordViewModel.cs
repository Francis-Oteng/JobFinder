using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace JobFinder.Models.ViewModels
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "The current password field is required.")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "The new password field is required.")]
        [StringLength(100, ErrorMessage = "The new password must be at least 8 characters long.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "The confirm password field is required.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm New Password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmNewPassword { get; set; }
    }
}
