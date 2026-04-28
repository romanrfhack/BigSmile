using BigSmile.Application.Features.Scheduling.Dtos;
using BigSmile.Application.Interfaces.Repositories;
using BigSmile.Domain.Entities;
using BigSmile.SharedKernel.Context;

namespace BigSmile.Application.Features.Scheduling.Commands
{
    public sealed record CreateReminderTemplateCommand(
        string Name,
        string Body);

    public sealed record UpdateReminderTemplateCommand(
        string Name,
        string Body);

    public interface IReminderTemplateCommandService
    {
        Task<ReminderTemplateDto> CreateAsync(
            CreateReminderTemplateCommand command,
            CancellationToken cancellationToken = default);
        Task<ReminderTemplateDto?> UpdateAsync(
            Guid id,
            UpdateReminderTemplateCommand command,
            CancellationToken cancellationToken = default);
        Task<bool> DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
    }

    public sealed class ReminderTemplateCommandService : IReminderTemplateCommandService
    {
        private readonly IReminderTemplateRepository _reminderTemplateRepository;
        private readonly ITenantContext _tenantContext;

        public ReminderTemplateCommandService(
            IReminderTemplateRepository reminderTemplateRepository,
            ITenantContext tenantContext)
        {
            _reminderTemplateRepository = reminderTemplateRepository ?? throw new ArgumentNullException(nameof(reminderTemplateRepository));
            _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        }

        public async Task<ReminderTemplateDto> CreateAsync(
            CreateReminderTemplateCommand command,
            CancellationToken cancellationToken = default)
        {
            var tenantId = GetRequiredTenantId();
            var actorUserId = GetRequiredUserId();
            var template = new ReminderTemplate(tenantId, command.Name, command.Body, actorUserId);

            await _reminderTemplateRepository.AddAsync(template, cancellationToken);
            return template.ToDto();
        }

        public async Task<ReminderTemplateDto?> UpdateAsync(
            Guid id,
            UpdateReminderTemplateCommand command,
            CancellationToken cancellationToken = default)
        {
            GetRequiredTenantId();
            var actorUserId = GetRequiredUserId();
            var template = await _reminderTemplateRepository.GetByIdAsync(id, cancellationToken);
            if (template == null)
            {
                return null;
            }

            template.Update(command.Name, command.Body, actorUserId);
            await _reminderTemplateRepository.UpdateAsync(template, cancellationToken);
            return template.ToDto();
        }

        public async Task<bool> DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
        {
            GetRequiredTenantId();
            var actorUserId = GetRequiredUserId();
            var template = await _reminderTemplateRepository.GetByIdAsync(id, cancellationToken);
            if (template == null)
            {
                return false;
            }

            template.Deactivate(actorUserId);
            await _reminderTemplateRepository.UpdateAsync(template, cancellationToken);
            return true;
        }

        private Guid GetRequiredTenantId()
        {
            var tenantIdValue = _tenantContext.GetTenantId();
            if (!Guid.TryParse(tenantIdValue, out var tenantId) || tenantId == Guid.Empty)
            {
                throw new InvalidOperationException("Reminder template operations require a resolved tenant context.");
            }

            return tenantId;
        }

        private Guid GetRequiredUserId()
        {
            var userIdValue = _tenantContext.GetUserId();
            if (!Guid.TryParse(userIdValue, out var userId) || userId == Guid.Empty)
            {
                throw new InvalidOperationException("Reminder template write operations require a resolved user context.");
            }

            return userId;
        }
    }
}
