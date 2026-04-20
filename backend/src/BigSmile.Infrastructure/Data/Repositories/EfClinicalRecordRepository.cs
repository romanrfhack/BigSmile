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
                .Include(clinicalRecord => clinicalRecord.Diagnoses)
                .SingleOrDefaultAsync(clinicalRecord => clinicalRecord.PatientId == patientId, cancellationToken);
        }

        public async Task AddAsync(ClinicalRecord clinicalRecord, CancellationToken cancellationToken = default)
        {
            _dbContext.ClinicalRecords.Add(clinicalRecord);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(ClinicalRecord clinicalRecord, CancellationToken cancellationToken = default)
        {
            if (_dbContext.Entry(clinicalRecord).State == EntityState.Detached)
            {
                _dbContext.ClinicalRecords.Attach(clinicalRecord);
                _dbContext.Entry(clinicalRecord).State = EntityState.Modified;
            }

            var persistedDiagnosisIds = await _dbContext.ClinicalDiagnoses
                .AsNoTracking()
                .Where(diagnosis => diagnosis.ClinicalRecordId == clinicalRecord.Id)
                .Select(diagnosis => diagnosis.Id)
                .ToListAsync(cancellationToken);

            foreach (var diagnosis in clinicalRecord.Diagnoses)
            {
                var entry = _dbContext.Entry(diagnosis);
                if (!persistedDiagnosisIds.Contains(diagnosis.Id))
                {
                    entry.State = EntityState.Added;
                }
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
