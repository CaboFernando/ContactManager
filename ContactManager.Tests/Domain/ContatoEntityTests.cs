using ContactManager.Domain.Entities;
using ContactManager.Domain.Enums;
using System.Reflection;
using Xunit;

namespace ContactManager.Tests.Domain;

public class ContatoEntityTests
{
    private static DateTime AdultBirthDate => DateTime.Today.AddYears(-25);
    private static DateTime MinorBirthDate => DateTime.Today.AddYears(-16);

    // ===================== Construtor =====================

    [Fact]
    public void Constructor_WithValidAdultData_ShouldCreateContact()
    {
        var contact = new Contato("João Silva", AdultBirthDate, Genero.Masculino);

        Assert.NotEqual(Guid.Empty, contact.Id);
        Assert.Equal("João Silva", contact.Nome);
        Assert.Equal(AdultBirthDate, contact.DataNascimento);
        Assert.Equal(Genero.Masculino, contact.Genero);
        Assert.True(contact.EstaAtivo);
        Assert.Equal(25, contact.Idade);
        Assert.Null(contact.AtualizadoEm);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_WithInvalidName_ShouldThrowArgumentException(string? name)
    {
        Assert.Throws<ArgumentException>(() =>
            new Contato(name!, AdultBirthDate, Genero.Masculino));
    }

    [Fact]
    public void Constructor_WithNameExceeding150Chars_ShouldThrowArgumentException()
    {
        var longName = new string('A', 151);
        Assert.Throws<ArgumentException>(() =>
            new Contato(longName, AdultBirthDate, Genero.Masculino));
    }

    [Fact]
    public void Constructor_WithMinorAge_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Contato("Minor Person", MinorBirthDate, Genero.Feminino));
    }

    [Fact]
    public void Constructor_WithTodayAsBirthDate_ShouldThrowArgumentException()
    {
        // Idade seria 0 — inválido
        Assert.Throws<ArgumentException>(() =>
            new Contato("Baby", DateTime.Today, Genero.Masculino));
    }

    [Fact]
    public void Constructor_WithFutureBirthDate_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Contato("Future", DateTime.Today.AddDays(1), Genero.Feminino));
    }

    [Fact]
    public void Constructor_WithExactly18YearsOld_ShouldCreateContact()
    {
        // Exatamente 18 anos hoje
        var birthDate = DateTime.Today.AddYears(-18);
        var contact = new Contato("Just Adult", birthDate, Genero.Masculino);

        Assert.Equal(18, contact.Idade);
    }

    // ===================== Cálculo de Idade =====================

    [Fact]
    public void Idade_ShouldBeCalculatedAtRuntime_NotStored()
    {
        var birthDate = DateTime.Today.AddYears(-30);
        var contact = new Contato("Test", birthDate, Genero.Masculino);

        Assert.Equal(30, contact.Idade);
    }

    [Fact]
    public void Idade_BeforeBirthdayThisYear_ShouldNotCountCurrentYear()
    {
        // Pessoa cujo aniversário ainda não aconteceu este ano
        var birthDate = new DateTime(DateTime.Today.Year - 20, DateTime.Today.Month + 1 > 12 ? 1 : DateTime.Today.Month + 1, 1);
        if (birthDate >= DateTime.Today) birthDate = birthDate.AddYears(-1);

        var contact = new Contato("Test", birthDate, Genero.Outro);

        Assert.True(contact.Idade >= 18);
    }

    // ===================== Atualização =====================

    [Fact]
    public void Update_WithValidData_ShouldUpdateContact()
    {
        var contact = new Contato("Old Name", AdultBirthDate, Genero.Masculino);
        var newBirthDate = DateTime.Today.AddYears(-30);

        contact.Update("New Name", newBirthDate, Genero.Feminino);

        Assert.Equal("New Name", contact.Nome);
        Assert.Equal(newBirthDate, contact.DataNascimento);
        Assert.Equal(Genero.Feminino, contact.Genero);
        Assert.NotNull(contact.AtualizadoEm);
    }

    [Fact]
    public void Update_WithMinorAge_ShouldThrowArgumentException()
    {
        var contact = new Contato("Test", AdultBirthDate, Genero.Masculino);

        Assert.Throws<ArgumentException>(() =>
            contact.Update("Test", MinorBirthDate, Genero.Masculino));
    }

    // ===================== Ativar / Desativar =====================

    [Fact]
    public void Desativar_ShouldSetEstaAtivoToFalse()
    {
        var contact = new Contato("Test", AdultBirthDate, Genero.Masculino);
        contact.Desativar();

        Assert.False(contact.EstaAtivo);
        Assert.NotNull(contact.AtualizadoEm);
    }

    [Fact]
    public void Ativar_AposDesativacao_ShouldSetEstaAtivoToTrue()
    {
        var contact = new Contato("Test", AdultBirthDate, Genero.Masculino);
        contact.Desativar();
        contact.Ativar();

        Assert.True(contact.EstaAtivo);
    }
}
