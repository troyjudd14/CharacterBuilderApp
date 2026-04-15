using CharacterBuilderApp.Models;

namespace CharacterBuilderApp.Repositories
{
    public interface ICharacterRepository
    {
        Task<List<Character>> GetAllAsync();
        Task<Character?> GetByIdAsync(int id);
        void Add(Character character);
        void Remove(Character character);
        Task<int> SaveChangesAsync();

    }
}