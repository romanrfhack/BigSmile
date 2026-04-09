using BigSmile.Application.Interfaces.Repositories;
using BigSmile.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BigSmile.Infrastructure.Data.Repositories
{
    public sealed class EfPatientRepository : IPatientRepository
    {
        private readonly AppDbContext _dbContext;

        public EfPatientRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<Patient?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Patients
                .FirstOrDefaultAsync(patient => patient.Id == id, cancellationToken);
        }

        public async Task<IReadOnlyList<Patient>> SearchAsync(
            string? searchTerm,
            bool includeInactive,
            int take,
            CancellationToken cancellationToken = default)
        {
            var query = _dbContext.Patients.AsQueryable();

            if (!includeInactive)
            {
                query = query.Where(patient => patient.IsActive);
            }

            var normalizedSearch = searchTerm?.Trim();
            if (!string.IsNullOrWhiteSpace(normalizedSearch))
            {
                var normalizedUpper = normalizedSearch.ToUpperInvariant();

                query = query.Where(patient =>
                    patient.FirstName.ToUpper().Contains(normalizedUpper) ||
                    patient.LastName.ToUpper().Contains(normalizedUpper) ||
                    (patient.Email != null && patient.Email.ToUpper().Contains(normalizedUpper)) ||
                    (patient.PrimaryPhone != null && patient.PrimaryPhone.Contains(normalizedSearch)));
            }

            return await query
                .OrderByDescending(patient => patient.IsActive)
                .ThenBy(patient => patient.LastName)
                .ThenBy(patient => patient.FirstName)
                .Take(take)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(Patient patient, CancellationToken cancellationToken = default)
        {
            _dbContext.Patients.Add(patient);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(Patient patient, CancellationToken cancellationToken = default)
        {
            _dbContext.Entry(patient).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
