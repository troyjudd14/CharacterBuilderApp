using System.ComponentModel.DataAnnotations;

namespace CharacterBuilderApp.Models
{
    public class Character
    {
        public int Id { get; set; }
        [Required]
        [StringLength(30, MinimumLength = 1)]
        public string Name { get; set; } = string.Empty;
        [Required]
        public CharacterClass? Class { get; set; }
        public int Level { get; set; } = 1;
        public int Experience { get; set; } = 0;

        public Character()
        {
        }

        public Character(string name, CharacterClass? characterClass)
        {
            Name = name;
            Class = characterClass;
        }
    }
}