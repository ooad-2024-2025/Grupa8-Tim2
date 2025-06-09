using System.ComponentModel.DataAnnotations;

namespace K_K.Models
{
    public enum VrstaPlacanja
    {
        [Display(Name = "Gotovina")]
        Gotovina,
        [Display(Name = "Kartica")]
        Kartica
    }
}