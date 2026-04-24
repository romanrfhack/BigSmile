using BigSmile.Application.Interfaces.Repositories;
using BigSmile.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BigSmile.Infrastructure.Data.Repositories
{
    public sealed class EfPatientDocumentRepository : IPatientDocumentRepository
    {
        private readonly AppDbContext _dbContext;

        public EfPatientDocumentRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<IReadOnlyList<PatientDocument>> ListActiveByPatientIdAsync(Guid patientId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.PatientDocuments
                .Where(patientDocument => patientDocument.PatientId == patientId && patientDocument.DeletedAtUtc == null)
                .ToListAsync(cancellationToken);
        }

        public async Task<PatientDocument?> GetActiveByIdAsync(Guid patientId, Guid documentId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.PatientDocuments
                .SingleOrDefaultAsync(
                    patientDocument => patientDocument.PatientId == patientId &&
                                       patientDocument.Id == documentId &&
                                       patientDocument.DeletedAtUtc == null,
                    cancellationToken);
        }

        public async Task AddAsync(PatientDocument patientDocument, CancellationToken cancellationToken = default)
        {
            _dbContext.PatientDocuments.Add(patientDocument);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(PatientDocument patientDocument, CancellationToken cancellationToken = default)
        {
            if (_dbContext.Entry(patientDocument).State == EntityState.Detached)
            {
                _dbContext.PatientDocuments.Update(patientDocument);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
