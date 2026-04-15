using CharacterBuilderApp.Models;
using Microsoft.EntityFrameworkCore;
using CharacterBuilderApp.Data;

namespace CharacterBuilderApp.Repositories
{
    public class CharacterRepository : ICharacterRepository
    {
        private readonly CharacterDbContext _context;

        public CharacterRepository(CharacterDbContext context)
        {
            _context = context;
        }

        public async Task<List<Character>> GetAllAsync()
        {
            return await _context.Characters.ToListAsync();
        }

        public async Task<Character?> GetByIdAsync(int id)
        {
            return await _context.Characters.FirstOrDefaultAsync(c => c.Id == id);
        }

        public void Add(Character character)
        {
            _context.Characters.Add(character);
        }

        public void Remove(Character character)
        {
            _context.Characters.Remove(character);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}