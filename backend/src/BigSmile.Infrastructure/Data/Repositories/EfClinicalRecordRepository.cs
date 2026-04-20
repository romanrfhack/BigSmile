using BigSmile.Application.Interfaces.Repositories;
using BigSmile.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BigSmile.Infrastructure.Data.Repositories
{
    public sealed class EfClinicalRecordRepository : IClinicalRecordRepository
    {
        private readonly AppDbContext _dbContext;

        public EfClinicalRecordRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<ClinicalRecord?> GetByPatientIdAsync(Guid patientId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.ClinicalRecords
                .Include(clinicalRecord => clinicalRecord.Allergies)
                .Include(clinicalRecord => clinicalRecord.Notes)
                .SingleOrDefaultAsync(clinicalRecord => clinicalRecord.PatientId == patientId, cancellationToken);
        }

        public async Task AddAsync(ClinicalRecord clinicalRecord, CancellationToken cancellationToken = default)
        {
            _dbContext.ClinicalRecords.Add(clinicalRecord);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(ClinicalRecord clinicalRecord, CancellationToken cancellationToken = default)
        {
            _dbContext.ChangeTracker.DetectChanges();
            _dbContext.Entry(clinicalRecord).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
