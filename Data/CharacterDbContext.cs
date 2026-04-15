using CharacterBuilderApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CharacterBuilderApp.Data
{
    public class CharacterDbContext : DbContext
    {
        public CharacterDbContext(DbContextOptions<CharacterDbContext> options) : base(options)
        {
        }

        public DbSet<Character> Characters { get; set; }
    }
}