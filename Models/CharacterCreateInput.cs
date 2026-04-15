using System.ComponentModel.DataAnnotations;

namespace CharacterBuilderApp.Models
{
    public class CharacterCreateInput
    {
        [Required]
        [StringLength(30, MinimumLength = 1)]
        public string Name { get; set; }

        [Required]
        public CharacterClass? Class { get; set; }
    }
}