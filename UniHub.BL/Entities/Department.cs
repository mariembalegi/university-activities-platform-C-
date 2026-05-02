using System.ComponentModel.DataAnnotations;

namespace UniHub.BL.Entities
{
    public enum Department
    {
        [Display(Name = "Génie Informatique")]
        GI = 1,

        [Display(Name = "Génie Civil")]
        GC = 2,

        [Display(Name = "Génie Électrique")]
        GE = 3,

        [Display(Name = "Génie Mécanique")]
        GM = 4,

        [Display(Name = "Génie des Technologies de l'Information et de la Communication")]
        GTIC = 5
    }
}
