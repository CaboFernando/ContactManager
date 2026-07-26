using ContactManager.Domain.Enums;
using System.Reflection;

namespace ContactManager.Application.DTOs;

public record ContatoDto(
    Guid Id,
    string Nome,
    DateTime DataNascimento,
    string Genero,
    int Idade,
    bool EstaAtivo,
    DateTime CriadoEm,
    DateTime? AtualizadoEm
);

public record CreateContatoDto(
    string Nome,
    DateTime DataNascimento,
    Genero Genero
);

public record UpdateContatoDto(
    string Nome,
    DateTime DataNascimento,
    Genero Genero
);

public record ContatoResponseDto<T>(
    bool Success,
    string Message,
    T? Data = default
);
