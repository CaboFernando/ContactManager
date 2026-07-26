using ContactManager.Application.DTOs;

namespace ContactManager.Application.Interfaces;

public interface IContatoService
{
    Task<ContatoResponseDto<IEnumerable<ContatoDto>>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task<ContatoResponseDto<ContatoDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ContatoResponseDto<ContatoDto>> CreateAsync(CreateContatoDto dto, CancellationToken cancellationToken = default);
    Task<ContatoResponseDto<ContatoDto>> UpdateAsync(Guid id, UpdateContatoDto dto, CancellationToken cancellationToken = default);
    Task<ContatoResponseDto<ContatoDto>> ActivateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ContatoResponseDto<ContatoDto>> DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ContatoResponseDto<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
