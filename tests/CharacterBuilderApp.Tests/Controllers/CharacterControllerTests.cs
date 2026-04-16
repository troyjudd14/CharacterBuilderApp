using CharacterBuilderApp.Controllers;
using CharacterBuilderApp.Models;
using CharacterBuilderApp.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CharacterBuilderApp.Tests.Controllers;

public class CharacterControllerTests
{
    private readonly Mock<ICharacterService> _mockService;
    private readonly CharacterController _controller;

    public CharacterControllerTests()
    {
        _mockService = new Mock<ICharacterService>();
        _controller = new CharacterController(_mockService.Object);
    }

    // ── Index ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Index_ReturnsViewResult_WithCharacterList()
    {
        // Arrange
        var characters = new List<Character>
        {
            new Character { Id = 1, Name = "Frodo", Class = CharacterClass.Rogue },
            new Character { Id = 2, Name = "Gimli", Class = CharacterClass.Warrior }
        };
        _mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(characters);

        // Act
        var result = await _controller.Index();

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().BeEquivalentTo(characters);
    }

    // ── Create GET ────────────────────────────────────────────────────────

    [Fact]
    public void Create_Get_ReturnsViewResult()
    {
        // Act
        var result = _controller.Create();

        // Assert
        result.Should().BeOfType<ViewResult>();
    }

    // ── Create POST ───────────────────────────────────────────────────────

    [Fact]
    public async Task Create_Post_WhenModelStateInvalid_ReturnsViewWithInput()
    {
        // Arrange
        _controller.ModelState.AddModelError("Name", "Required");
        var input = new CharacterCreateInput { Name = "", Class = CharacterClass.Warrior };

        // Act
        var result = await _controller.Create(input);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().Be(input);
        _mockService.Verify(s => s.CreateAsync(It.IsAny<CharacterCreateInput>()), Times.Never);
    }

    [Fact]
    public async Task Create_Post_WhenSucceeds_RedirectsToIndex()
    {
        // Arrange
        var input = new CharacterCreateInput { Name = "Aragorn", Class = CharacterClass.Warrior };
        _mockService.Setup(s => s.CreateAsync(input))
            .ReturnsAsync(new Character { Id = 1, Name = "Aragorn", Class = CharacterClass.Warrior });

        // Act
        var result = await _controller.Create(input);

        // Assert
        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be("Index");
    }

    [Fact]
    public async Task Create_Post_WhenServiceThrowsCharacterPersistenceException_ReturnsViewWithModelError()
    {
        // Arrange
        var input = new CharacterCreateInput { Name = "Aragorn", Class = CharacterClass.Warrior };
        _mockService.Setup(s => s.CreateAsync(input))
            .ThrowsAsync(new CharacterPersistenceException("DB failure"));

        // Act
        var result = await _controller.Create(input);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().Be(input);
        _controller.ModelState.IsValid.Should().BeFalse();
    }

    // ── Details ───────────────────────────────────────────────────────────

    [Fact]
    public async Task Details_WhenCharacterNotFound_ReturnsNotFound()
    {
        // Arrange
        _mockService.Setup(s => s.GetByIdAsync(99)).ReturnsAsync((Character?)null);

        // Act
        var result = await _controller.Details(99);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Details_WhenCharacterFound_ReturnsViewWithCharacter()
    {
        // Arrange
        var character = new Character { Id = 1, Name = "Legolas", Class = CharacterClass.Archer };
        _mockService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(character);

        // Act
        var result = await _controller.Details(1);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().Be(character);
    }

    // ── Edit GET ──────────────────────────────────────────────────────────

    [Fact]
    public async Task Edit_Get_WhenCharacterNotFound_ReturnsNotFound()
    {
        // Arrange
        _mockService.Setup(s => s.GetEditInputAsync(99)).ReturnsAsync((CharacterEditInput?)null);

        // Act
        var result = await _controller.Edit(99);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Edit_Get_WhenCharacterFound_ReturnsViewWithEditInput()
    {
        // Arrange
        var editInput = new CharacterEditInput { Id = 1, Name = "Boromir", Class = CharacterClass.Warrior };
        _mockService.Setup(s => s.GetEditInputAsync(1)).ReturnsAsync(editInput);

        // Act
        var result = await _controller.Edit(1);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().Be(editInput);
    }

    // ── Edit POST ─────────────────────────────────────────────────────────

    [Fact]
    public async Task Edit_Post_WhenModelStateInvalid_ReturnsViewWithInput()
    {
        // Arrange
        _controller.ModelState.AddModelError("Name", "Required");
        var input = new CharacterEditInput { Id = 1, Name = "", Class = CharacterClass.Warrior };

        // Act
        var result = await _controller.Edit(input);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().Be(input);
        _mockService.Verify(s => s.UpdateAsync(It.IsAny<CharacterEditInput>()), Times.Never);
    }

    [Fact]
    public async Task Edit_Post_WhenUpdated_RedirectsToDetails()
    {
        // Arrange
        var input = new CharacterEditInput { Id = 1, Name = "Updated", Class = CharacterClass.Mage };
        _mockService.Setup(s => s.UpdateAsync(input)).ReturnsAsync(CharacterUpdateResult.Updated);

        // Act
        var result = await _controller.Edit(input);

        // Assert
        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be("Details");
        redirect.RouteValues!["id"].Should().Be(1);
    }

    [Fact]
    public async Task Edit_Post_WhenUnchanged_RedirectsToDetails()
    {
        // Arrange
        var input = new CharacterEditInput { Id = 1, Name = "Same", Class = CharacterClass.Mage };
        _mockService.Setup(s => s.UpdateAsync(input)).ReturnsAsync(CharacterUpdateResult.Unchanged);

        // Act
        var result = await _controller.Edit(input);

        // Assert
        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be("Details");
    }

    [Fact]
    public async Task Edit_Post_WhenNotFound_ReturnsNotFound()
    {
        // Arrange
        var input = new CharacterEditInput { Id = 99, Name = "Ghost", Class = CharacterClass.Mage };
        _mockService.Setup(s => s.UpdateAsync(input)).ReturnsAsync(CharacterUpdateResult.NotFound);

        // Act
        var result = await _controller.Edit(input);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Edit_Post_WhenFailed_ReturnsObjectResultWithProblem()
    {
        // Arrange
        var input = new CharacterEditInput { Id = 1, Name = "Fail", Class = CharacterClass.Mage };
        _mockService.Setup(s => s.UpdateAsync(input)).ReturnsAsync(CharacterUpdateResult.Failed);

        // Act
        var result = await _controller.Edit(input);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    // ── Delete GET ────────────────────────────────────────────────────────

    [Fact]
    public async Task Delete_Get_WhenCharacterNotFound_ReturnsNotFound()
    {
        // Arrange
        _mockService.Setup(s => s.GetByIdAsync(99)).ReturnsAsync((Character?)null);

        // Act
        var result = await _controller.Delete(99);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Delete_Get_WhenCharacterFound_ReturnsViewWithCharacter()
    {
        // Arrange
        var character = new Character { Id = 1, Name = "Saruman", Class = CharacterClass.Mage };
        _mockService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(character);

        // Act
        var result = await _controller.Delete(1);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().Be(character);
    }

    // ── Delete POST ───────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteConfirmed_WhenDeleted_RedirectsToIndex()
    {
        // Arrange
        _mockService.Setup(s => s.DeleteAsync(1)).ReturnsAsync(CharacterDeleteResult.Deleted);

        // Act
        var result = await _controller.DeleteConfirmed(1);

        // Assert
        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be("Index");
    }

    [Fact]
    public async Task DeleteConfirmed_WhenNotFound_ReturnsNotFound()
    {
        // Arrange
        _mockService.Setup(s => s.DeleteAsync(99)).ReturnsAsync(CharacterDeleteResult.NotFound);

        // Act
        var result = await _controller.DeleteConfirmed(99);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task DeleteConfirmed_WhenFailed_Returns500()
    {
        // Arrange
        _mockService.Setup(s => s.DeleteAsync(1)).ReturnsAsync(CharacterDeleteResult.Failed);

        // Act
        var result = await _controller.DeleteConfirmed(1);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }
}