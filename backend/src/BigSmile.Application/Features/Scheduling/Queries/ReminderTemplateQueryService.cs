using BigSmile.Application.Features.Branches.Services;
using BigSmile.Application.Features.Scheduling.Dtos;
using BigSmile.Application.Interfaces.Repositories;
using BigSmile.SharedKernel.Context;

namespace BigSmile.Application.Features.Scheduling.Queries
{
    public interface IReminderTemplateQueryService
    {
        Task<IReadOnlyList<ReminderTemplateDto>> ListAsync(
            bool includeInactive = false,
            CancellationToken cancellationToken = default);
        Task<ReminderTemplatePreviewDto?> PreviewAsync(
            Guid templateId,
            Guid appointmentId,
            CancellationToken cancellationToken = default);
    }

    public sealed class ReminderTemplateQueryService : IReminderTemplateQueryService
    {
        private readonly IReminderTemplateRepository _reminderTemplateRepository;
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly ITenantRepository _tenantRepository;
        private readonly IBranchAccessService _branchAccessService;
        private readonly ITenantContext _tenantContext;

        public ReminderTemplateQueryService(
            IReminderTemplateRepository reminderTemplateRepository,
            IAppointmentRepository appointmentRepository,
            ITenantRepository tenantRepository,
            IBranchAccessService branchAccessService,
            ITenantContext tenantContext)
        {
            _reminderTemplateRepository = reminderTemplateRepository ?? throw new ArgumentNullException(nameof(reminderTemplateRepository));
            _appointmentRepository = appointmentRepository ?? throw new ArgumentNullException(nameof(appointmentRepository));
            _tenantRepository = tenantRepository ?? throw new ArgumentNullException(nameof(tenantRepository));
            _branchAccessService = branchAccessService ?? throw new ArgumentNullException(nameof(branchAccessService));
            _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        }

        public async Task<IReadOnlyList<ReminderTemplateDto>> ListAsync(
            bool includeInactive = false,
            CancellationToken cancellationToken = default)
        {
            EnsureTenantContext();

            var templates = await _reminderTemplateRepository.ListAsync(includeInactive, cancellationToken);
            return templates.Select(template => template.ToDto()).ToArray();
        }

        public async Task<ReminderTemplatePreviewDto?> PreviewAsync(
            Guid templateId,
            Guid appointmentId,
            CancellationToken cancellationToken = default)
        {
            var tenantId = EnsureTenantContext();
            var template = await _reminderTemplateRepository.GetByIdAsync(templateId, cancellationToken);
            if (template == null || !template.IsActive)
            {
                return null;
            }

            var appointment = await _appointmentRepository.GetByIdAsync(appointmentId, cancellationToken);
            if (appointment == null)
            {
                return null;
            }

            var branch = await _branchAccessService.GetAccessibleBranchAsync(appointment.BranchId, cancellationToken);
            if (branch == null || !branch.IsActive)
            {
                return null;
            }

            var tenant = await _tenantRepository.GetByIdAsync(tenantId, cancellationToken);
            if (tenant == null)
            {
                throw new InvalidOperationException("Reminder template preview requires an available tenant.");
            }

            var renderResult = ReminderTemplateRenderer.Render(template, appointment, tenant, branch);
            return new ReminderTemplatePreviewDto(
                template.Id,
                appointment.Id,
                renderResult.RenderedBody,
                renderResult.UnknownPlaceholders);
        }

        private Guid EnsureTenantContext()
        {
            var tenantIdValue = _tenantContext.GetTenantId();
            if (!Guid.TryParse(tenantIdValue, out var tenantId) || tenantId == Guid.Empty)
            {
                throw new InvalidOperationException("Reminder template operations require a resolved tenant context.");
            }

            return tenantId;
        }
    }
}
