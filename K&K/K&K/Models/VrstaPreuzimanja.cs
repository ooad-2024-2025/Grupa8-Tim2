using System.ComponentModel.DataAnnotations;

namespace K_K.Models
{
    public enum VrstaPreuzimanja
    {
        [Display(Name = "Lično")]
        Licno,
        [Display(Name = "Dostava")]
        Dostava
    }
}