using ContactManager.Domain.Entities;

namespace ContactManager.Domain.Interfaces;

public interface IContactRepository
{
    Task<IEnumerable<Contact>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task<Contact?> GetActiveByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Contact?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAndBirthDateAsync(string name, DateTime birthDate, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task AddAsync(Contact contact, CancellationToken cancellationToken = default);
    void Update(Contact contact);
    void Delete(Contact contact);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
