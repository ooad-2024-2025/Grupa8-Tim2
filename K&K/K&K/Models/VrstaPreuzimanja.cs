using System.ComponentModel.DataAnnotations;

namespace K_K.Models
{
    public enum VrstaPreuzimanja
    {
        [Display(Name = "Licno")]
        Licno,
        [Display(Name = "Dostava")]
        Dostava
    }
}