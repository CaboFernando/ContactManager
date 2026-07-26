using ContactManager.Domain.Entities;
using ContactManager.Domain.Interfaces;
using ContactManager.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace ContactManager.Infrastructure.Repositories;

public class ContatoRepository : IContatoRepository
{
    private readonly AppDbContext _context;

    public ContatoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Contato>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Contacts
            .AsNoTracking()
            .Where(c => c.EstaAtivo)
            .OrderBy(c => c.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<Contato?> GetActiveByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Contacts
            .FirstOrDefaultAsync(c => c.Id == id && c.EstaAtivo, cancellationToken);
    }

    public async Task<Contato?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Contacts
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsByNameAndBirthDateAsync(string nome, DateTime dataNascimento, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Contacts
            .AsNoTracking()
            .Where(c => c.Nome.ToLower() == nome.ToLower() && c.DataNascimento == dataNascimento.Date);

        if (excludeId.HasValue)
            query = query.Where(c => c.Id != excludeId.Value);

        return await query.AnyAsync(cancellationToken);
    }

    public async Task AddAsync(Contato contato, CancellationToken cancellationToken = default)
    {
        await _context.Contacts.AddAsync(contato, cancellationToken);
    }

    public void Update(Contato contato)
    {
        _context.Contacts.Update(contato);
    }

    public void Delete(Contato contato)
    {
        _context.Contacts.Remove(contato);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
