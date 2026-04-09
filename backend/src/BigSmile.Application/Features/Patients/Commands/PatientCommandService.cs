using BigSmile.Application.Features.Patients.Dtos;
using BigSmile.Application.Interfaces.Repositories;
using BigSmile.Domain.Entities;
using BigSmile.SharedKernel.Context;

namespace BigSmile.Application.Features.Patients.Commands
{
    public sealed record SavePatientCommand(
        string FirstName,
        string LastName,
        DateOnly DateOfBirth,
        string? PrimaryPhone,
        string? Email,
        bool IsActive,
        string? ResponsiblePartyName,
        string? ResponsiblePartyRelationship,
        string? ResponsiblePartyPhone);

    public interface IPatientCommandService
    {
        Task<PatientDetailDto> CreateAsync(SavePatientCommand command, CancellationToken cancellationToken = default);
        Task<PatientDetailDto?> UpdateAsync(Guid id, SavePatientCommand command, CancellationToken cancellationToken = default);
    }

    public sealed class PatientCommandService : IPatientCommandService
    {
        private readonly IPatientRepository _patientRepository;
        private readonly ITenantContext _tenantContext;

        public PatientCommandService(
            IPatientRepository patientRepository,
            ITenantContext tenantContext)
        {
            _patientRepository = patientRepository ?? throw new ArgumentNullException(nameof(patientRepository));
            _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        }

        public async Task<PatientDetailDto> CreateAsync(SavePatientCommand command, CancellationToken cancellationToken = default)
        {
            var tenantId = GetRequiredTenantId();
            var patient = new Patient(
                tenantId,
                command.FirstName,
                command.LastName,
                command.DateOfBirth,
                command.PrimaryPhone,
                command.Email,
                command.IsActive,
                command.ResponsiblePartyName,
                command.ResponsiblePartyRelationship,
                command.ResponsiblePartyPhone);

            await _patientRepository.AddAsync(patient, cancellationToken);
            return patient.ToDetailDto();
        }

        public async Task<PatientDetailDto?> UpdateAsync(Guid id, SavePatientCommand command, CancellationToken cancellationToken = default)
        {
            GetRequiredTenantId();
            var patient = await _patientRepository.GetByIdAsync(id, cancellationToken);
            if (patient == null)
            {
                return null;
            }

            patient.UpdateProfile(
                command.FirstName,
                command.LastName,
                command.DateOfBirth,
                command.PrimaryPhone,
                command.Email,
                command.IsActive,
                command.ResponsiblePartyName,
                command.ResponsiblePartyRelationship,
                command.ResponsiblePartyPhone);

            await _patientRepository.UpdateAsync(patient, cancellationToken);
            return patient.ToDetailDto();
        }

        private Guid GetRequiredTenantId()
        {
            var tenantIdValue = _tenantContext.GetTenantId();
            if (!Guid.TryParse(tenantIdValue, out var tenantId) || tenantId == Guid.Empty)
            {
                throw new InvalidOperationException("Patient operations require a resolved tenant context.");
            }

            return tenantId;
        }
    }
}
