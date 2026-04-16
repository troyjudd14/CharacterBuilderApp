using CharacterBuilderApp.Models;
using Microsoft.EntityFrameworkCore;
using CharacterBuilderApp.Repositories;

namespace CharacterBuilderApp.Services
{
    public class CharacterService : ICharacterService
    {
        private readonly ICharacterRepository _repository;

        public CharacterService(ICharacterRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<Character>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<Character?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<Character> CreateAsync(CharacterCreateInput input)
        {
            var character = new Character
            {
                Name = input.Name,
                Class = input.Class,
                Level = 1, // Ensure level is set to 1 for new characters
                Experience = 0 // Ensure experience is set to 0 for new characters
            };
            _repository.Add(character);
            var rows = await _repository.SaveChangesAsync();
            if (rows < 1)
            {
                throw new CharacterPersistenceException("Failed to create character.");
            }

            return character;
        }

        public async Task<CharacterEditInput?> GetEditInputAsync(int id)
        {
            var character = await _repository.GetByIdAsync(id);
            if (character == null) return null;

            return new CharacterEditInput
            {
                Id = character.Id,
                Name = character.Name,
                Class = character.Class
            };
        }

        public async Task<CharacterUpdateResult> UpdateAsync(CharacterEditInput input)
        {
            var character = await _repository.GetByIdAsync(input.Id);
            if (character == null) return CharacterUpdateResult.NotFound;

            if (character.Name == input.Name && character.Class == input.Class)
            {
                return CharacterUpdateResult.Unchanged;
            }

            // These properties are tracked by EF, so updating them will mark the entity as modified
            character.Name = input.Name;
            character.Class = input.Class;

            try
            {
                var rows = await _repository.SaveChangesAsync();
                return rows > 0 ? CharacterUpdateResult.Updated : CharacterUpdateResult.Failed;
            }
            catch (DbUpdateException)
            {
                return CharacterUpdateResult.Failed;
            }
        }

        public async Task<CharacterDeleteResult> DeleteAsync(int id)
        {
            var character = await _repository.GetByIdAsync(id);
            if (character == null) return CharacterDeleteResult.NotFound;

            _repository.Remove(character);
            try
            {
                var rows = await _repository.SaveChangesAsync();
                return rows > 0 ? CharacterDeleteResult.Deleted : CharacterDeleteResult.Failed;
            }
            catch (DbUpdateException)
            {
                return CharacterDeleteResult.Failed;
            }
        }

    }
}