using CharacterBuilderApp.Models;
using CharacterBuilderApp.Repositories;
using CharacterBuilderApp.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using FluentAssertions;

namespace CharacterBuilderApp.Tests.Services;

public class CharacterServiceTests
{
    private readonly Mock<ICharacterRepository> _mockRepo;
    private readonly CharacterService _service;

    public CharacterServiceTests()
    {
        _mockRepo = new Mock<ICharacterRepository>();
        _service = new CharacterService(_mockRepo.Object);
    }

    // ── CreateAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_WhenSaveSucceeds_ReturnsCharacterWithDefaults()
    {
        // Arrange
        _mockRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        var input = new CharacterCreateInput { Name = "Test Warrior", Class = CharacterClass.Warrior };

        Character? addedCharacter = null;
        _mockRepo.Setup(r => r.Add(It.IsAny<Character>()))
            .Callback<Character>(c => addedCharacter = c);

        // Act
        var result = await _service.CreateAsync(input);

        // Assert
        result.Name.Should().Be(input.Name);
        result.Class.Should().Be(input.Class);
        result.Level.Should().Be(1);
        result.Experience.Should().Be(0);

        addedCharacter.Should().NotBeNull();
        addedCharacter!.Name.Should().Be(input.Name);
        addedCharacter.Class.Should().Be(input.Class);
        addedCharacter.Level.Should().Be(1);
        addedCharacter.Experience.Should().Be(0);

        _mockRepo.Verify(r => r.Add(It.IsAny<Character>()), Times.Once);
        _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenSaveFails_ThrowsCharacterPersistenceException()
    {
        // Arrange
        _mockRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(0);

        var input = new CharacterCreateInput { Name = "Test Mage", Class = CharacterClass.Mage };

        // Act
        Func<Task> act = async () => await _service.CreateAsync(input);

        // Assert
        await act.Should().ThrowAsync<CharacterPersistenceException>();
        _mockRepo.Verify(r => r.Add(It.IsAny<Character>()), Times.Once);
        _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    // ── UpdateAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_WhenCharacterNotFound_ReturnsNotFound()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Character?)null);

        var input = new CharacterEditInput { Id = 999, Name = "Updated Name", Class = CharacterClass.Bard };

        // Act
        var result = await _service.UpdateAsync(input);

        // Assert
        result.Should().Be(CharacterUpdateResult.NotFound);
        _mockRepo.Verify(r => r.GetByIdAsync(input.Id), Times.Once);
        _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenNoChanges_ReturnsUnchanged()
    {
        // Arrange
        var existingCharacter = new Character { Id = 1, Name = "Existing Mage", Class = CharacterClass.Mage, Level = 5, Experience = 1500 };
        _mockRepo.Setup(r => r.GetByIdAsync(existingCharacter.Id)).ReturnsAsync(existingCharacter);

        var input = new CharacterEditInput { Id = existingCharacter.Id, Name = existingCharacter.Name, Class = existingCharacter.Class };

        // Act
        var result = await _service.UpdateAsync(input);

        // Assert
        result.Should().Be(CharacterUpdateResult.Unchanged);
        _mockRepo.Verify(r => r.GetByIdAsync(input.Id), Times.Once);
        _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenCharacterExistsAndChanged_ReturnsUpdated()
    {
        // Arrange
        var existingCharacter = new Character { Id = 1, Name = "Existing Character", Class = CharacterClass.Mage, Level = 3, Experience = 800 };
        _mockRepo.Setup(r => r.GetByIdAsync(existingCharacter.Id)).ReturnsAsync(existingCharacter);
        _mockRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        var input = new CharacterEditInput { Id = existingCharacter.Id, Name = "Updated Character", Class = CharacterClass.Warrior };

        // Act
        var result = await _service.UpdateAsync(input);

        // Assert
        result.Should().Be(CharacterUpdateResult.Updated);
        existingCharacter.Name.Should().Be(input.Name);
        existingCharacter.Class.Should().Be(input.Class);
        _mockRepo.Verify(r => r.GetByIdAsync(input.Id), Times.Once);
        _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenCharacterExistsButSaveFails_ReturnsFailed()
    {
        // Arrange
        var existingCharacter = new Character { Id = 1, Name = "Existing Character", Class = CharacterClass.Mage, Level = 3, Experience = 800 };
        _mockRepo.Setup(r => r.GetByIdAsync(existingCharacter.Id)).ReturnsAsync(existingCharacter);
        _mockRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(0);

        var input = new CharacterEditInput { Id = existingCharacter.Id, Name = "Updated Character", Class = CharacterClass.Warrior };

        // Act
        var result = await _service.UpdateAsync(input);

        // Assert
        result.Should().Be(CharacterUpdateResult.Failed);
        existingCharacter.Name.Should().Be(input.Name);
        existingCharacter.Class.Should().Be(input.Class);
        _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenSaveThrowsDbUpdateException_ReturnsFailed()
    {
        // Arrange
        var existingCharacter = new Character { Id = 1, Name = "Existing Character", Class = CharacterClass.Mage, Level = 3, Experience = 800 };
        _mockRepo.Setup(r => r.GetByIdAsync(existingCharacter.Id)).ReturnsAsync(existingCharacter);
        _mockRepo.Setup(r => r.SaveChangesAsync()).ThrowsAsync(new DbUpdateException("DB error"));

        var input = new CharacterEditInput { Id = existingCharacter.Id, Name = "Updated Name", Class = CharacterClass.Warrior };

        // Act
        var result = await _service.UpdateAsync(input);

        // Assert
        result.Should().Be(CharacterUpdateResult.Failed);
        _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    // ── DeleteAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_WhenCharacterNotFound_ReturnsNotFound()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Character?)null);

        // Act
        var result = await _service.DeleteAsync(999);

        // Assert
        result.Should().Be(CharacterDeleteResult.NotFound);
        _mockRepo.Verify(r => r.Remove(It.IsAny<Character>()), Times.Never);
        _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenCharacterExistsAndSaveSucceeds_ReturnsDeleted()
    {
        // Arrange
        var existingCharacter = new Character { Id = 1, Name = "To Delete", Class = CharacterClass.Rogue };
        _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingCharacter);
        _mockRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _service.DeleteAsync(1);

        // Assert
        result.Should().Be(CharacterDeleteResult.Deleted);
        _mockRepo.Verify(r => r.Remove(existingCharacter), Times.Once);
        _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenSaveReturnsZero_ReturnsFailed()
    {
        // Arrange
        var existingCharacter = new Character { Id = 1, Name = "To Delete", Class = CharacterClass.Rogue };
        _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingCharacter);
        _mockRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(0);

        // Act
        var result = await _service.DeleteAsync(1);

        // Assert
        result.Should().Be(CharacterDeleteResult.Failed);
        _mockRepo.Verify(r => r.Remove(existingCharacter), Times.Once);
        _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenSaveThrowsDbUpdateException_ReturnsFailed()
    {
        // Arrange
        var existingCharacter = new Character { Id = 1, Name = "To Delete", Class = CharacterClass.Rogue };
        _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingCharacter);
        _mockRepo.Setup(r => r.SaveChangesAsync()).ThrowsAsync(new DbUpdateException("DB error"));

        // Act
        var result = await _service.DeleteAsync(1);

        // Assert
        result.Should().Be(CharacterDeleteResult.Failed);
        _mockRepo.Verify(r => r.Remove(existingCharacter), Times.Once);
        _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    // ── GetEditInputAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task GetEditInputAsync_WhenCharacterNotFound_ReturnsNull()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Character?)null);

        // Act
        var result = await _service.GetEditInputAsync(999);

        // Assert
        result.Should().BeNull();
        _mockRepo.Verify(r => r.GetByIdAsync(999), Times.Once);
    }

    [Fact]
    public async Task GetEditInputAsync_WhenCharacterFound_ReturnsMappedInput()
    {
        // Arrange
        var existingCharacter = new Character { Id = 1, Name = "Legolas", Class = CharacterClass.Archer };
        _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingCharacter);

        // Act
        var result = await _service.GetEditInputAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(existingCharacter.Id);
        result.Name.Should().Be(existingCharacter.Name);
        result.Class.Should().Be(existingCharacter.Class);
    }

    // ── GetAllAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_ReturnsAllCharacters()
    {
        // Arrange
        var characters = new List<Character>
        {
            new Character { Id = 1, Name = "Frodo", Class = CharacterClass.Rogue },
            new Character { Id = 2, Name = "Gimli", Class = CharacterClass.Warrior }
        };
        _mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(characters);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().ContainSingle(c => c.Name == "Frodo");
        result.Should().ContainSingle(c => c.Name == "Gimli");
    }

    // ── GetByIdAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_WhenCharacterExists_ReturnsCharacter()
    {
        // Arrange
        var character = new Character { Id = 1, Name = "Aragorn", Class = CharacterClass.Warrior };
        _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(character);

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("Aragorn");
    }

    [Fact]
    public async Task GetByIdAsync_WhenCharacterNotFound_ReturnsNull()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Character?)null);

        // Act
        var result = await _service.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }
}