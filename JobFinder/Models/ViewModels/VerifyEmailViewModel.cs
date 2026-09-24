using System.ComponentModel.DataAnnotations;

namespace JobFinder.Models.ViewModels
{
    public class VerifyEmailViewModel
    {
        [Required(ErrorMessage = "The email field is required.")]
        [EmailAddress(ErrorMessage = "The email format is invalid.")]
        public string Email { get; set; }
    }
}
