using ContactManager.Domain.Entities;

namespace ContactManager.Domain.Interfaces;

public interface IContatoRepository
{
    Task<IEnumerable<Contato>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task<Contato?> GetActiveByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Contato?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAndBirthDateAsync(string nome, DateTime dataNascimento, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task AddAsync(Contato contato, CancellationToken cancellationToken = default);
    void Update(Contato contato);
    void Delete(Contato contato);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
