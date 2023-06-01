using System.ComponentModel.DataAnnotations;

namespace Trains.Models
{
    public class RegisterBindingModel
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }



}