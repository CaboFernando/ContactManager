using ContactManager.Application.DTOs;
using ContactManager.Application.Services;
using ContactManager.Domain.Entities;
using ContactManager.Domain.Enums;
using ContactManager.Domain.Interfaces;
using Moq;
using System.Reflection;
using Xunit;

namespace ContactManager.Tests.Application;

public class ContatoServiceTests
{
    private readonly Mock<IContatoRepository> _repositoryMock;
    private readonly ContatoService _service;

    private static DateTime AdultBirthDate => DateTime.Today.AddYears(-25);

    public ContatoServiceTests()
    {
        _repositoryMock = new Mock<IContatoRepository>();
        _service = new ContatoService(_repositoryMock.Object);
    }

    // ===================== GetAll =====================

    [Fact]
    public async Task GetAllActiveAsync_ShouldReturnOnlyActiveContacts()
    {
        var contacts = new List<Contato>
        {
            new("Alice", AdultBirthDate, Genero.Feminino),
            new("Bob", DateTime.Today.AddYears(-30), Genero.Masculino)
        };
        _repositoryMock.Setup(r => r.GetAllActiveAsync(default)).ReturnsAsync(contacts);

        var result = await _service.GetAllActiveAsync();

        Assert.True(result.Success);
        Assert.Equal(2, result.Data!.Count());
    }

    // ===================== GetById =====================

    [Fact]
    public async Task GetByIdAsync_WhenActiveContactExists_ShouldReturnContact()
    {
        var contact = new Contato("Alice", AdultBirthDate, Genero.Feminino);
        _repositoryMock.Setup(r => r.GetActiveByIdAsync(contact.Id, default)).ReturnsAsync(contact);

        var result = await _service.GetByIdAsync(contact.Id);

        Assert.True(result.Success);
        Assert.Equal(contact.Id, result.Data!.Id);
        Assert.Equal(25, result.Data.Idade);
    }

    [Fact]
    public async Task GetByIdAsync_WhenContactIsInactive_ShouldReturnFailure()
    {
        var id = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetActiveByIdAsync(id, default)).ReturnsAsync((Contato?)null);

        var result = await _service.GetByIdAsync(id);

        Assert.False(result.Success);
        Assert.Contains("não encontrado.", result.Message);
    }

    // ===================== Create =====================

    [Fact]
    public async Task CreateAsync_WithValidAdultContact_ShouldCreateSuccessfully()
    {
        var dto = new CreateContatoDto("Alice", AdultBirthDate, Genero.Feminino);
        _repositoryMock.Setup(r => r.ExistsByNameAndBirthDateAsync(dto.Nome, dto.DataNascimento, null, default)).ReturnsAsync(false);

        var result = await _service.CreateAsync(dto);

        Assert.True(result.Success);
        Assert.Equal("Alice", result.Data!.Nome);
        Assert.Equal(25, result.Data.Idade);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Contato>(), default), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenDuplicateContact_ShouldReturnFailure()
    {
        var dto = new CreateContatoDto("Alice", AdultBirthDate, Genero.Feminino);
        _repositoryMock.Setup(r => r.ExistsByNameAndBirthDateAsync(dto.Nome, dto.DataNascimento, null, default)).ReturnsAsync(true);

        var result = await _service.CreateAsync(dto);

        Assert.False(result.Success);
        Assert.Contains("Um contato com o mesmo nome e data de nascimento já existe.", result.Message);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Contato>(), default), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WithMinorAge_ShouldReturnFailure()
    {
        var minorBirthDate = DateTime.Today.AddYears(-16);
        var dto = new CreateContatoDto("Minor", minorBirthDate, Genero.Masculino);
        _repositoryMock.Setup(r => r.ExistsByNameAndBirthDateAsync(dto.Nome, dto.DataNascimento, null, default)).ReturnsAsync(false);

        var result = await _service.CreateAsync(dto);

        Assert.False(result.Success);
        Assert.Contains("18", result.Message);
    }

    [Fact]
    public async Task CreateAsync_WithTodayAsBirthDate_ShouldReturnFailure()
    {
        var dto = new CreateContatoDto("Baby", DateTime.Today, Genero.Masculino);
        _repositoryMock.Setup(r => r.ExistsByNameAndBirthDateAsync(dto.Nome, dto.DataNascimento, null, default)).ReturnsAsync(false);

        var result = await _service.CreateAsync(dto);

        Assert.False(result.Success);
    }

    // ===================== Update =====================

    [Fact]
    public async Task UpdateAsync_WithValidData_ShouldUpdateContact()
    {
        var contact = new Contato("Old Name", AdultBirthDate, Genero.Masculino);
        var dto = new UpdateContatoDto("New Name", DateTime.Today.AddYears(-30), Genero.Feminino);
        _repositoryMock.Setup(r => r.GetActiveByIdAsync(contact.Id, default)).ReturnsAsync(contact);
        _repositoryMock.Setup(r => r.ExistsByNameAndBirthDateAsync(dto.Nome, dto.DataNascimento, contact.Id, default)).ReturnsAsync(false);

        var result = await _service.UpdateAsync(contact.Id, dto);

        Assert.True(result.Success);
        Assert.Equal("New Name", result.Data!.Nome);
        _repositoryMock.Verify(r => r.Update(It.IsAny<Contato>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenContactNotFound_ShouldReturnFailure()
    {
        var id = Guid.NewGuid();
        var dto = new UpdateContatoDto("Name", AdultBirthDate, Genero.Masculino);
        _repositoryMock.Setup(r => r.GetActiveByIdAsync(id, default)).ReturnsAsync((Contato?)null);

        var result = await _service.UpdateAsync(id, dto);

        Assert.False(result.Success);
        Assert.Contains("não encontrado.", result.Message);
    }

    // ===================== Activate / Deactivate =====================

    [Fact]
    public async Task DeactivateAsync_WhenActiveContact_ShouldDeactivate()
    {
        var contact = new Contato("Test", AdultBirthDate, Genero.Masculino);
        _repositoryMock.Setup(r => r.GetActiveByIdAsync(contact.Id, default)).ReturnsAsync(contact);

        var result = await _service.DeactivateAsync(contact.Id);

        Assert.True(result.Success);
        _repositoryMock.Verify(r => r.Update(It.IsAny<Contato>()), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task ActivateAsync_WhenInactiveContact_ShouldActivate()
    {
        var contact = new Contato("Test", AdultBirthDate, Genero.Masculino);
        contact.Desativar();
        _repositoryMock.Setup(r => r.GetByIdAsync(contact.Id, default)).ReturnsAsync(contact);

        var result = await _service.ActivateAsync(contact.Id);

        Assert.True(result.Success);
        _repositoryMock.Verify(r => r.Update(It.IsAny<Contato>()), Times.Once);
    }

    [Fact]
    public async Task ActivateAsync_WhenAlreadyActive_ShouldReturnFailure()
    {
        var contact = new Contato("Test", AdultBirthDate, Genero.Masculino);
        _repositoryMock.Setup(r => r.GetByIdAsync(contact.Id, default)).ReturnsAsync(contact);

        var result = await _service.ActivateAsync(contact.Id);

        Assert.False(result.Success);
        Assert.Contains("Contato já está ativo.", result.Message);
    }

    // ===================== Delete =====================

    [Fact]
    public async Task DeleteAsync_WhenContactExists_ShouldDelete()
    {
        var contact = new Contato("Test", AdultBirthDate, Genero.Masculino);
        _repositoryMock.Setup(r => r.GetByIdAsync(contact.Id, default)).ReturnsAsync(contact);

        var result = await _service.DeleteAsync(contact.Id);

        Assert.True(result.Success);
        _repositoryMock.Verify(r => r.Delete(It.IsAny<Contato>()), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenContactNotFound_ShouldReturnFailure()
    {
        var id = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetByIdAsync(id, default)).ReturnsAsync((Contato?)null);

        var result = await _service.DeleteAsync(id);

        Assert.False(result.Success);
        _repositoryMock.Verify(r => r.Delete(It.IsAny<Contato>()), Times.Never);
    }
}
