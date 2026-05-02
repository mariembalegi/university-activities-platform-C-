using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using UniHub.BL.Entities;

namespace UniHub.UI.ViewModels
{
    public class CreateUserViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string? Email    { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Prénom")]
        public string? FirstName { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Nom")]
        public string? LastName { get; set; }

        [Required]
        [Display(Name = "Département")]
        public Department Department { get; set; }

        [Required]
        [Display(Name = "Rôle")]
        public string? Role { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Le {0} doit contenir au moins {2} caractères.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Mot de passe")]
        public string? Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmer le mot de passe")]
        [Compare("Password", ErrorMessage = "Les mots de passe ne correspondent pas.")]
        public string? ConfirmPassword { get; set; }

        [Display(Name = "Photo de profil")]
        public IFormFile? ProfilePicture { get; set; }
    }

    public class EditUserViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Display(Name = "Prénom")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Display(Name = "Nom")]
        public string LastName { get; set; } = string.Empty;

        [Display(Name = "Département")]
        public Department? Department { get; set; }

        [Required]
        [Display(Name = "Rôle")]
        public string Role { get; set; } = string.Empty;

        [Display(Name = "Photo de profil")]
        public IFormFile? ProfilePicture { get; set; }

        public byte[]? ExistingProfilePicture { get; set; }
    }

    public class UserViewModel
    {
        public string? Id { get; set; }
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public Department? Department { get; set; }
        public byte[]? ProfilePicture { get; set; }
        public List<string>? Roles { get; set; }
    }

    public class DashboardStatsViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalStudents { get; set; }
        public int TotalDepartmentAdmins { get; set; }
        public int TotalActivities { get; set; }
    }
}
