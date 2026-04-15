using CharacterBuilderApp.Models;

namespace CharacterBuilderApp.Services
{
    public interface ICharacterService
    {
        Task<List<Character>> GetAllAsync();
        Task<Character?> GetByIdAsync(int id);
        Task<Character> CreateAsync(CharacterCreateInput input);
        Task<CharacterEditInput?> GetEditInputAsync(int id);
        Task<CharacterUpdateResult> UpdateAsync(CharacterEditInput input);
        Task<CharacterDeleteResult> DeleteAsync(int id);
    }
}