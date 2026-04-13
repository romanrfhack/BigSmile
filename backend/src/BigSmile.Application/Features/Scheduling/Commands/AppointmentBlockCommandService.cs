using BigSmile.Application.Features.Branches.Services;
using BigSmile.Application.Features.Scheduling.Dtos;
using BigSmile.Application.Interfaces.Repositories;
using BigSmile.Domain.Entities;
using BigSmile.SharedKernel.Context;

namespace BigSmile.Application.Features.Scheduling.Commands
{
    public sealed record CreateAppointmentBlockCommand(
        Guid BranchId,
        DateTime StartsAt,
        DateTime EndsAt,
        string? Label);

    public interface IAppointmentBlockCommandService
    {
        Task<AppointmentBlockSummaryDto> CreateAsync(
            CreateAppointmentBlockCommand command,
            CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }

    public sealed class AppointmentBlockCommandService : IAppointmentBlockCommandService
    {
        private readonly IAppointmentBlockRepository _appointmentBlockRepository;
        private readonly IBranchAccessService _branchAccessService;
        private readonly ITenantContext _tenantContext;

        public AppointmentBlockCommandService(
            IAppointmentBlockRepository appointmentBlockRepository,
            IBranchAccessService branchAccessService,
            ITenantContext tenantContext)
        {
            _appointmentBlockRepository = appointmentBlockRepository ?? throw new ArgumentNullException(nameof(appointmentBlockRepository));
            _branchAccessService = branchAccessService ?? throw new ArgumentNullException(nameof(branchAccessService));
            _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        }

        public async Task<AppointmentBlockSummaryDto> CreateAsync(
            CreateAppointmentBlockCommand command,
            CancellationToken cancellationToken = default)
        {
            var tenantId = GetRequiredTenantId();
            var branch = await GetRequiredActiveBranchAsync(command.BranchId, cancellationToken);

            var appointmentBlock = new AppointmentBlock(
                tenantId,
                branch.Id,
                command.StartsAt,
                command.EndsAt,
                command.Label);

            await _appointmentBlockRepository.AddAsync(appointmentBlock, cancellationToken);
            return appointmentBlock.ToSummaryDto();
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            GetRequiredTenantId();

            var appointmentBlock = await _appointmentBlockRepository.GetByIdAsync(id, cancellationToken);
            if (appointmentBlock == null)
            {
                return false;
            }

            await GetRequiredActiveBranchAsync(appointmentBlock.BranchId, cancellationToken);
            await _appointmentBlockRepository.DeleteAsync(appointmentBlock, cancellationToken);

            return true;
        }

        private async Task<Branch> GetRequiredActiveBranchAsync(Guid branchId, CancellationToken cancellationToken)
        {
            var branch = await _branchAccessService.GetAccessibleBranchAsync(branchId, cancellationToken);
            if (branch == null)
            {
                throw new InvalidOperationException("The requested branch is not accessible in the current tenant scope.");
            }

            if (!branch.IsActive)
            {
                throw new InvalidOperationException("Blocked time slots can only be managed in active branches.");
            }

            return branch;
        }

        private Guid GetRequiredTenantId()
        {
            var tenantIdValue = _tenantContext.GetTenantId();
            if (!Guid.TryParse(tenantIdValue, out var tenantId) || tenantId == Guid.Empty)
            {
                throw new InvalidOperationException("Scheduling operations require a resolved tenant context.");
            }

            return tenantId;
        }
    }
}
