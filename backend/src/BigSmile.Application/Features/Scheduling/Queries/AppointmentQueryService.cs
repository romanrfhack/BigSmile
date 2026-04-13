using BigSmile.Application.Features.Branches.Services;
using BigSmile.Application.Features.Scheduling.Dtos;
using BigSmile.Application.Interfaces.Repositories;
using BigSmile.SharedKernel.Context;

namespace BigSmile.Application.Features.Scheduling.Queries
{
    public interface IAppointmentQueryService
    {
        Task<CalendarViewDto> GetCalendarAsync(
            Guid branchId,
            DateOnly startDate,
            int days,
            CancellationToken cancellationToken = default);
    }

    public sealed class AppointmentQueryService : IAppointmentQueryService
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IAppointmentBlockRepository _appointmentBlockRepository;
        private readonly IBranchAccessService _branchAccessService;
        private readonly ITenantContext _tenantContext;

        public AppointmentQueryService(
            IAppointmentRepository appointmentRepository,
            IAppointmentBlockRepository appointmentBlockRepository,
            IBranchAccessService branchAccessService,
            ITenantContext tenantContext)
        {
            _appointmentRepository = appointmentRepository ?? throw new ArgumentNullException(nameof(appointmentRepository));
            _appointmentBlockRepository = appointmentBlockRepository ?? throw new ArgumentNullException(nameof(appointmentBlockRepository));
            _branchAccessService = branchAccessService ?? throw new ArgumentNullException(nameof(branchAccessService));
            _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        }

        public async Task<CalendarViewDto> GetCalendarAsync(
            Guid branchId,
            DateOnly startDate,
            int days,
            CancellationToken cancellationToken = default)
        {
            EnsureTenantContext();

            var branch = await _branchAccessService.GetAccessibleBranchAsync(branchId, cancellationToken);
            if (branch == null)
            {
                throw new InvalidOperationException("The requested branch is not accessible in the current tenant scope.");
            }

            var normalizedDays = days == 1 ? 1 : 7;
            var rangeStart = startDate.ToDateTime(TimeOnly.MinValue);
            var rangeEnd = rangeStart.AddDays(normalizedDays);

            var appointments = await _appointmentRepository.GetCalendarAsync(
                branch.Id,
                rangeStart,
                rangeEnd,
                cancellationToken);
            var appointmentBlocks = await _appointmentBlockRepository.GetCalendarAsync(
                branch.Id,
                rangeStart,
                rangeEnd,
                cancellationToken);

            var calendarDays = Enumerable.Range(0, normalizedDays)
                .Select(offset =>
                {
                    var date = startDate.AddDays(offset);
                    var items = appointments
                        .Where(appointment => DateOnly.FromDateTime(appointment.StartsAt) == date)
                        .OrderBy(appointment => appointment.StartsAt)
                        .ThenBy(appointment => appointment.Patient.FullName)
                        .Select(appointment => appointment.ToSummaryDto())
                        .ToArray();
                    var blockedSlots = appointmentBlocks
                        .Where(appointmentBlock => DateOnly.FromDateTime(appointmentBlock.StartsAt) == date)
                        .OrderBy(appointmentBlock => appointmentBlock.StartsAt)
                        .ThenBy(appointmentBlock => appointmentBlock.EndsAt)
                        .Select(appointmentBlock => appointmentBlock.ToSummaryDto())
                        .ToArray();

                    return new CalendarDayDto(date, items, blockedSlots);
                })
                .ToArray();

            return new CalendarViewDto(branch.Id, startDate, normalizedDays, calendarDays);
        }

        private void EnsureTenantContext()
        {
            var tenantIdValue = _tenantContext.GetTenantId();
            if (!Guid.TryParse(tenantIdValue, out var tenantId) || tenantId == Guid.Empty)
            {
                throw new InvalidOperationException("Scheduling queries require a resolved tenant context.");
            }
        }
    }
}
