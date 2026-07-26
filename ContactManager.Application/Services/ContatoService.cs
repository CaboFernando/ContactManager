using ContactManager.Application.DTOs;
using ContactManager.Application.Interfaces;
using ContactManager.Domain.Entities;
using ContactManager.Domain.Interfaces;

namespace ContactManager.Application.Services;

public class ContatoService : IContatoService
{
    private readonly IContatoRepository _repository;

    public ContatoService(IContatoRepository repository)
    {
        _repository = repository;
    }

    public async Task<ContatoResponseDto<IEnumerable<ContatoDto>>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        var contatos = await _repository.GetAllActiveAsync(cancellationToken);
        var dtos = contatos.Select(MapToDto);
        return new ContatoResponseDto<IEnumerable<ContatoDto>>(true, "Contatos recuperados com sucesso.", dtos);
    }

    public async Task<ContatoResponseDto<ContatoDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Detail view also considers only active contacts (per spec)
        var contato = await _repository.GetActiveByIdAsync(id, cancellationToken);

        if (contato is null)
            return new ContatoResponseDto<ContatoDto>(false, $"Contato ativo com id '{id}' não encontrado.");

        return new ContatoResponseDto<ContatoDto>(true, "Contato recuperado com sucesso.", MapToDto(contato));
    }

    public async Task<ContatoResponseDto<ContatoDto>> CreateAsync(CreateContatoDto dto, CancellationToken cancellationToken = default)
    {
        var alreadyExists = await _repository.ExistsByNameAndBirthDateAsync(dto.Nome, dto.DataNascimento, cancellationToken: cancellationToken);
        if (alreadyExists)
            return new ContatoResponseDto<ContatoDto>(false, "Um contato com o mesmo nome e data de nascimento já existe.");

        try
        {
            var contato = new Contato(dto.Nome, dto.DataNascimento, dto.Genero);
            await _repository.AddAsync(contato, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            return new ContatoResponseDto<ContatoDto>(true, "Contato criado com sucesso.", MapToDto(contato));
        }
        catch (ArgumentException ex)
        {
            return new ContatoResponseDto<ContatoDto>(false, ex.Message);
        }
    }

    public async Task<ContatoResponseDto<ContatoDto>> UpdateAsync(Guid id, UpdateContatoDto dto, CancellationToken cancellationToken = default)
    {
        var contato = await _repository.GetActiveByIdAsync(id, cancellationToken);

        if (contato is null)
            return new ContatoResponseDto<ContatoDto>(false, $"Contato ativo com id '{id}' não encontrado.");

        var alreadyExists = await _repository.ExistsByNameAndBirthDateAsync(dto.Nome, dto.DataNascimento, id, cancellationToken);
        if (alreadyExists)
            return new ContatoResponseDto<ContatoDto>(false, "Um contato com o mesmo nome e data de nascimento já existe.");

        try
        {
            contato.Update(dto.Nome, dto.DataNascimento, dto.Genero);
            _repository.Update(contato);
            await _repository.SaveChangesAsync(cancellationToken);

            return new ContatoResponseDto<ContatoDto>(true, "Contato atualizado com sucesso.", MapToDto(contato));
        }
        catch (ArgumentException ex)
        {
            return new ContatoResponseDto<ContatoDto>(false, ex.Message);
        }
    }

    public async Task<ContatoResponseDto<ContatoDto>> ActivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Activate can act on any contato (active or inactive)
        var contato = await _repository.GetByIdAsync(id, cancellationToken);

        if (contato is null)
            return new ContatoResponseDto<ContatoDto>(false, $"Contato com id '{id}' não encontrado.");

        if (contato.EstaAtivo)
            return new ContatoResponseDto<ContatoDto>(false, "Contato já está ativo.");

        contato.Ativar();
        _repository.Update(contato);
        await _repository.SaveChangesAsync(cancellationToken);

        return new ContatoResponseDto<ContatoDto>(true, "Contato ativado com sucesso.", MapToDto(contato));
    }

    public async Task<ContatoResponseDto<ContatoDto>> DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var contato = await _repository.GetActiveByIdAsync(id, cancellationToken);

        if (contato is null)
            return new ContatoResponseDto<ContatoDto>(false, $"Contato ativo com id '{id}' não encontrado.");

        contato.Desativar();
        _repository.Update(contato);
        await _repository.SaveChangesAsync(cancellationToken);

        return new ContatoResponseDto<ContatoDto>(true, "Contato desativado com sucesso.", MapToDto(contato));
    }

    public async Task<ContatoResponseDto<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var contato = await _repository.GetByIdAsync(id, cancellationToken);

        if (contato is null)
            return new ContatoResponseDto<bool>(false, $"Contato com id '{id}' não encontrado.");

        _repository.Delete(contato);
        await _repository.SaveChangesAsync(cancellationToken);

        return new ContatoResponseDto<bool>(true, "Contato deletado com sucesso.", true);
    }

    private static ContatoDto MapToDto(Contato contato) => new(
        contato.Id,
        contato.Nome,
        contato.DataNascimento,
        contato.Genero.ToString(),
        contato.Idade,
        contato.EstaAtivo,
        contato.CriadoEm,
        contato.AtualizadoEm
    );
}
