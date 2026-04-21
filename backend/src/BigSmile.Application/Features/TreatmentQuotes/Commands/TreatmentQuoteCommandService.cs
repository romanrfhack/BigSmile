using BigSmile.Application.Features.TreatmentQuotes.Dtos;
using BigSmile.Application.Interfaces.Repositories;
using BigSmile.Domain.Entities;
using BigSmile.SharedKernel.Context;

namespace BigSmile.Application.Features.TreatmentQuotes.Commands
{
    public sealed record UpdateTreatmentQuoteItemPriceCommand(
        decimal UnitPrice);

    public sealed record ChangeTreatmentQuoteStatusCommand(
        string Status);

    public interface ITreatmentQuoteCommandService
    {
        Task<TreatmentQuoteDetailDto> CreateAsync(
            Guid patientId,
            CancellationToken cancellationToken = default);

        Task<TreatmentQuoteDetailDto?> UpdateItemUnitPriceAsync(
            Guid patientId,
            Guid quoteItemId,
            UpdateTreatmentQuoteItemPriceCommand command,
            CancellationToken cancellationToken = default);

        Task<TreatmentQuoteDetailDto?> ChangeStatusAsync(
            Guid patientId,
            ChangeTreatmentQuoteStatusCommand command,
            CancellationToken cancellationToken = default);
    }

    public sealed class TreatmentQuoteCommandService : ITreatmentQuoteCommandService
    {
        private readonly ITreatmentQuoteRepository _treatmentQuoteRepository;
        private readonly ITreatmentPlanRepository _treatmentPlanRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly ITenantContext _tenantContext;

        public TreatmentQuoteCommandService(
            ITreatmentQuoteRepository treatmentQuoteRepository,
            ITreatmentPlanRepository treatmentPlanRepository,
            IPatientRepository patientRepository,
            ITenantContext tenantContext)
        {
            _treatmentQuoteRepository = treatmentQuoteRepository ?? throw new ArgumentNullException(nameof(treatmentQuoteRepository));
            _treatmentPlanRepository = treatmentPlanRepository ?? throw new ArgumentNullException(nameof(treatmentPlanRepository));
            _patientRepository = patientRepository ?? throw new ArgumentNullException(nameof(patientRepository));
            _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        }

        public async Task<TreatmentQuoteDetailDto> CreateAsync(
            Guid patientId,
            CancellationToken cancellationToken = default)
        {
            var tenantId = GetRequiredTenantId();
            var actorUserId = GetRequiredUserId();
            var patient = await GetRequiredPatientAsync(patientId, cancellationToken);

            EnsurePatientBelongsToTenant(patient, tenantId);

            var treatmentPlan = await _treatmentPlanRepository.GetByPatientIdAsync(patientId, cancellationToken);
            if (treatmentPlan is null)
            {
                throw new InvalidOperationException("An existing treatment plan is required before creating a treatment quote.");
            }

            if (treatmentPlan.Items.Count == 0)
            {
                throw new InvalidOperationException("Treatment quote creation requires the existing treatment plan to contain at least one item.");
            }

            var existingQuote = await _treatmentQuoteRepository.GetByTreatmentPlanIdAsync(treatmentPlan.Id, cancellationToken);
            if (existingQuote is not null)
            {
                throw new InvalidOperationException("A treatment quote already exists for the requested treatment plan.");
            }

            var treatmentQuote = new TreatmentQuote(
                tenantId,
                patient.Id,
                treatmentPlan.Id,
                treatmentPlan.Items,
                actorUserId);

            await _treatmentQuoteRepository.AddAsync(treatmentQuote, cancellationToken);
            return treatmentQuote.ToDetailDto();
        }

        public async Task<TreatmentQuoteDetailDto?> UpdateItemUnitPriceAsync(
            Guid patientId,
            Guid quoteItemId,
            UpdateTreatmentQuoteItemPriceCommand command,
            CancellationToken cancellationToken = default)
        {
            var tenantId = GetRequiredTenantId();
            var actorUserId = GetRequiredUserId();
            var patient = await GetRequiredPatientAsync(patientId, cancellationToken);

            EnsurePatientBelongsToTenant(patient, tenantId);

            var treatmentQuote = await _treatmentQuoteRepository.GetByPatientIdAsync(patientId, cancellationToken);
            if (treatmentQuote is null)
            {
                return null;
            }

            var changed = treatmentQuote.UpdateItemUnitPrice(quoteItemId, command.UnitPrice, actorUserId);
            if (changed)
            {
                await _treatmentQuoteRepository.UpdateAsync(treatmentQuote, cancellationToken);
            }

            return treatmentQuote.ToDetailDto();
        }

        public async Task<TreatmentQuoteDetailDto?> ChangeStatusAsync(
            Guid patientId,
            ChangeTreatmentQuoteStatusCommand command,
            CancellationToken cancellationToken = default)
        {
            var tenantId = GetRequiredTenantId();
            var actorUserId = GetRequiredUserId();
            var patient = await GetRequiredPatientAsync(patientId, cancellationToken);

            EnsurePatientBelongsToTenant(patient, tenantId);

            var treatmentQuote = await _treatmentQuoteRepository.GetByPatientIdAsync(patientId, cancellationToken);
            if (treatmentQuote is null)
            {
                return null;
            }

            var newStatus = ParseStatus(command.Status);
            var changed = treatmentQuote.ChangeStatus(newStatus, actorUserId);
            if (changed)
            {
                await _treatmentQuoteRepository.UpdateAsync(treatmentQuote, cancellationToken);
            }

            return treatmentQuote.ToDetailDto();
        }

        private async Task<Patient> GetRequiredPatientAsync(Guid patientId, CancellationToken cancellationToken)
        {
            var patient = await _patientRepository.GetByIdAsync(patientId, cancellationToken);
            if (patient is null)
            {
                throw new InvalidOperationException("The requested patient is not available in the current tenant scope.");
            }

            return patient;
        }

        private static void EnsurePatientBelongsToTenant(Patient patient, Guid tenantId)
        {
            if (patient.TenantId != tenantId)
            {
                throw new InvalidOperationException("Treatment quotes can only reference patients from the current tenant.");
            }
        }

        private Guid GetRequiredTenantId()
        {
            var tenantIdValue = _tenantContext.GetTenantId();
            if (!Guid.TryParse(tenantIdValue, out var tenantId) || tenantId == Guid.Empty)
            {
                throw new InvalidOperationException("Treatment quote operations require a resolved tenant context.");
            }

            return tenantId;
        }

        private Guid GetRequiredUserId()
        {
            var userIdValue = _tenantContext.GetUserId();
            if (!Guid.TryParse(userIdValue, out var userId) || userId == Guid.Empty)
            {
                throw new InvalidOperationException("Treatment quote write operations require a resolved user context.");
            }

            return userId;
        }

        private static TreatmentQuoteStatus ParseStatus(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
            {
                throw new ArgumentException("Treatment quote status is required.", nameof(status));
            }

            if (!Enum.TryParse<TreatmentQuoteStatus>(status.Trim(), ignoreCase: true, out var parsedStatus) ||
                !Enum.IsDefined(parsedStatus))
            {
                throw new ArgumentException(
                    "Treatment quote status must be one of: Draft, Proposed, or Accepted.",
                    nameof(status));
            }

            return parsedStatus;
        }
    }
}
