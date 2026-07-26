using ContactManager.Domain.Enums;
using System.Reflection;

namespace ContactManager.Domain.Entities;

public class Contato
{
    private const int MinimumAge = 18;

    public Guid Id { get; private set; }
    public string Nome { get; private set; }
    public DateTime DataNascimento { get; private set; }
    public Genero Genero { get; private set; }
    public bool EstaAtivo { get; private set; }
    public DateTime CriadoEm { get; private set; }
    public DateTime? AtualizadoEm { get; private set; }

    /// <summary>
    /// Idade é calculada em tempo de execução — não armazenada no banco de dados.
    /// </summary>
    public int Idade => CalcularIdade(DataNascimento);

    protected Contato() { }

    public Contato(string nome, DateTime dataNascimento, Genero genero)
    {
        ValidarNome(nome);
        ValidarDataNascimento(dataNascimento);

        Id = Guid.NewGuid();
        Nome = nome;
        DataNascimento = dataNascimento;
        Genero = genero;
        EstaAtivo = true;
        CriadoEm = DateTime.UtcNow;
    }

    public void Update(string nome, DateTime dataNascimento, Genero genero)
    {
        ValidarNome(nome);
        ValidarDataNascimento(dataNascimento);

        Nome = nome;
        DataNascimento = dataNascimento;
        Genero = genero;
        AtualizadoEm = DateTime.UtcNow;
    }

    public void Ativar()
    {
        EstaAtivo = true;
        AtualizadoEm = DateTime.UtcNow;
    }

    public void Desativar()
    {
        EstaAtivo = false;
        AtualizadoEm = DateTime.UtcNow;
    }

    private static int CalcularIdade(DateTime dataNascimento)
    {
        var hoje = DateTime.Today;
        var idade = hoje.Year - dataNascimento.Year;

        if (dataNascimento.Date > hoje.AddYears(-idade))
            idade--;

        return idade;
    }

    private static void ValidarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Contact name is required.", nameof(nome));

        if (nome.Length > 150)
            throw new ArgumentException("Contact name must not exceed 150 characters.", nameof(nome));
    }

    private static void ValidarDataNascimento(DateTime dataNascimento)
    {
        if (dataNascimento.Date >= DateTime.Today)
            throw new ArgumentException("Birth date must be earlier than today.", nameof(dataNascimento));

        var idade = CalcularIdade(dataNascimento);

        if (idade == 0)
            throw new ArgumentException("Age cannot be zero.", nameof(dataNascimento));

        if (idade < MinimumAge)
            throw new ArgumentException($"Contact must be at least {MinimumAge} years old. Calculated age: {idade}.", nameof(dataNascimento));
    }
}
