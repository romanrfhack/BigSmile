using BigSmile.Application.Interfaces.Repositories;
using BigSmile.Domain.Entities;
using BigSmile.SharedKernel;
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
                .Include(clinicalRecord => clinicalRecord.SnapshotHistory)
                .Include(clinicalRecord => clinicalRecord.MedicalAnswers)
                .Include(clinicalRecord => clinicalRecord.Encounters)
                    .ThenInclude(encounter => encounter.ClinicalNote)
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

            var persistedAllergyIds = await _dbContext.Set<ClinicalAllergyEntry>()
                .AsNoTracking()
                .Where(allergy => allergy.ClinicalRecordId == clinicalRecord.Id)
                .Select(allergy => allergy.Id)
                .ToListAsync(cancellationToken);

            var persistedNoteIds = await _dbContext.Set<ClinicalNote>()
                .AsNoTracking()
                .Where(note => note.ClinicalRecordId == clinicalRecord.Id)
                .Select(note => note.Id)
                .ToListAsync(cancellationToken);

            var persistedSnapshotHistoryIds = await _dbContext.Set<ClinicalSnapshotHistoryEntry>()
                .AsNoTracking()
                .Where(entry => entry.ClinicalRecordId == clinicalRecord.Id)
                .Select(entry => entry.Id)
                .ToListAsync(cancellationToken);

            var persistedMedicalAnswerIds = await _dbContext.ClinicalMedicalAnswers
                .AsNoTracking()
                .Where(answer => answer.ClinicalRecordId == clinicalRecord.Id)
                .Select(answer => answer.Id)
                .ToListAsync(cancellationToken);

            var persistedEncounterIds = await _dbContext.ClinicalEncounters
                .AsNoTracking()
                .Where(encounter => encounter.ClinicalRecordId == clinicalRecord.Id)
                .Select(encounter => encounter.Id)
                .ToListAsync(cancellationToken);

            MarkNewChildrenAsAdded(clinicalRecord.Diagnoses, persistedDiagnosisIds);
            MarkNewChildrenAsAdded(clinicalRecord.Allergies, persistedAllergyIds);
            MarkNewChildrenAsAdded(clinicalRecord.Notes, persistedNoteIds);
            MarkNewChildrenAsAdded(clinicalRecord.SnapshotHistory, persistedSnapshotHistoryIds);
            MarkNewChildrenAsAdded(clinicalRecord.MedicalAnswers, persistedMedicalAnswerIds);
            MarkNewChildrenAsAdded(clinicalRecord.Encounters, persistedEncounterIds);

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        private void MarkNewChildrenAsAdded<TEntity>(IEnumerable<TEntity> children, IReadOnlyCollection<Guid> persistedIds)
            where TEntity : class
        {
            foreach (var child in children)
            {
                if (child is not Entity<Guid> entity)
                {
                    continue;
                }

                var entry = _dbContext.Entry(child);
                if (!persistedIds.Contains(entity.Id))
                {
                    entry.State = EntityState.Added;
                }
            }
        }
    }
}
