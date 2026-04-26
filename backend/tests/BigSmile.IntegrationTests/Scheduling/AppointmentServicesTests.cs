using BigSmile.Application.Authorization;
using BigSmile.Application.Features.Branches.Services;
using BigSmile.Application.Features.Scheduling.Commands;
using BigSmile.Application.Features.Scheduling.Queries;
using BigSmile.Domain.Entities;
using BigSmile.Infrastructure.Context;
using BigSmile.Infrastructure.Data;
using BigSmile.Infrastructure.Data.Repositories;
using BigSmile.SharedKernel.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BigSmile.IntegrationTests.Scheduling
{
    public class AppointmentServicesTests
    {
        [Fact]
        public async Task CreateAsync_CreatesAppointmentInsideResolvedTenantAndAssignedBranch()
        {
            var databaseName = Guid.NewGuid().ToString();
            var seedData = await SeedTenantWithUserAsync(databaseName);
            var patient = await SeedPatientAsync(databaseName, seedData.Tenant.Id, "Ana", "Lopez");
            var tenantContext = CreateTenantContext(seedData.User.Id, seedData.Tenant.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = CreateCommandService(context, tenantContext);

            var appointment = await commandService.CreateAsync(new CreateAppointmentCommand(
                seedData.PrimaryBranch.Id,
                patient.Id,
                new DateTime(2026, 4, 14, 9, 0, 0),
                new DateTime(2026, 4, 14, 9, 30, 0),
                "Initial consultation"));

            Assert.Equal(seedData.PrimaryBranch.Id, appointment.BranchId);
            Assert.Equal(patient.Id, appointment.PatientId);
            Assert.Equal("Ana Lopez", appointment.PatientFullName);
            Assert.Equal("Scheduled", appointment.Status);
            Assert.Equal("Pending", appointment.ConfirmationStatus);
            Assert.Null(appointment.ConfirmedAtUtc);
            Assert.Null(appointment.ConfirmedByUserId);

            await using var verificationContext = CreateContext(databaseName, new TenantContext());
            var storedAppointment = await verificationContext.Appointments.SingleAsync();

            Assert.Equal(seedData.Tenant.Id, storedAppointment.TenantId);
            Assert.Equal(seedData.PrimaryBranch.Id, storedAppointment.BranchId);
            Assert.Equal(patient.Id, storedAppointment.PatientId);
            Assert.Equal(AppointmentConfirmationStatus.Pending, storedAppointment.ConfirmationStatus);
            Assert.Null(storedAppointment.ConfirmedAtUtc);
            Assert.Null(storedAppointment.ConfirmedByUserId);
        }

        [Fact]
        public async Task GetCalendarAsync_ReturnsOnlyCurrentTenantBranchAppointments()
        {
            var databaseName = Guid.NewGuid().ToString();
            var tenantASeed = await SeedTenantWithUserAsync(databaseName, tenantName: "Tenant A", tenantSubdomain: "tenant-a");
            var tenantBSeed = await SeedTenantWithUserAsync(databaseName, tenantName: "Tenant B", tenantSubdomain: "tenant-b");
            var patientA = await SeedPatientAsync(databaseName, tenantASeed.Tenant.Id, "Ana", "Lopez");
            var patientB = await SeedPatientAsync(databaseName, tenantBSeed.Tenant.Id, "Bruno", "Garcia");

            await SeedAppointmentAsync(
                databaseName,
                tenantASeed.Tenant.Id,
                tenantASeed.PrimaryBranch.Id,
                patientA.Id,
                new DateTime(2026, 4, 14, 9, 0, 0),
                new DateTime(2026, 4, 14, 9, 30, 0));
            await SeedAppointmentAsync(
                databaseName,
                tenantBSeed.Tenant.Id,
                tenantBSeed.PrimaryBranch.Id,
                patientB.Id,
                new DateTime(2026, 4, 14, 10, 0, 0),
                new DateTime(2026, 4, 14, 10, 30, 0));

            var tenantContext = CreateTenantContext(tenantASeed.User.Id, tenantASeed.Tenant.Id);
            await using var context = CreateContext(databaseName, tenantContext);
            var queryService = CreateQueryService(context, tenantContext);

            var calendar = await queryService.GetCalendarAsync(
                tenantASeed.PrimaryBranch.Id,
                new DateOnly(2026, 4, 14),
                1);

            Assert.Single(calendar.CalendarDays);
            Assert.Single(calendar.CalendarDays[0].Appointments);
            Assert.Equal("Ana Lopez", calendar.CalendarDays[0].Appointments[0].PatientFullName);
        }

        [Fact]
        public async Task CreateAsync_BlocksPatientFromAnotherTenant()
        {
            var databaseName = Guid.NewGuid().ToString();
            var tenantASeed = await SeedTenantWithUserAsync(databaseName, tenantName: "Tenant A", tenantSubdomain: "tenant-a");
            var tenantBSeed = await SeedTenantWithUserAsync(databaseName, tenantName: "Tenant B", tenantSubdomain: "tenant-b");
            var foreignPatient = await SeedPatientAsync(databaseName, tenantBSeed.Tenant.Id, "Bruno", "Garcia");
            var tenantContext = CreateTenantContext(tenantASeed.User.Id, tenantASeed.Tenant.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = CreateCommandService(context, tenantContext);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => commandService.CreateAsync(
                new CreateAppointmentCommand(
                    tenantASeed.PrimaryBranch.Id,
                    foreignPatient.Id,
                    new DateTime(2026, 4, 14, 9, 0, 0),
                    new DateTime(2026, 4, 14, 9, 30, 0),
                    null)));

            Assert.Contains("patient is not available in the current tenant scope", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task CreateAsync_BlocksBranchOutsideAssignedScope_ForTenantUser()
        {
            var databaseName = Guid.NewGuid().ToString();
            var seedData = await SeedTenantWithUserAsync(databaseName);
            var patient = await SeedPatientAsync(databaseName, seedData.Tenant.Id, "Ana", "Lopez");
            var tenantContext = CreateTenantContext(seedData.User.Id, seedData.Tenant.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = CreateCommandService(context, tenantContext);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => commandService.CreateAsync(
                new CreateAppointmentCommand(
                    seedData.SecondaryBranch.Id,
                    patient.Id,
                    new DateTime(2026, 4, 14, 11, 0, 0),
                    new DateTime(2026, 4, 14, 11, 30, 0),
                    null)));

            Assert.Contains("branch is not accessible", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task RescheduleAsync_UpdatesStoredSchedule_WithinTenantScope()
        {
            var databaseName = Guid.NewGuid().ToString();
            var seedData = await SeedTenantWithUserAsync(databaseName);
            var patient = await SeedPatientAsync(databaseName, seedData.Tenant.Id, "Ana", "Lopez");
            var appointment = await SeedAppointmentAsync(
                databaseName,
                seedData.Tenant.Id,
                seedData.PrimaryBranch.Id,
                patient.Id,
                new DateTime(2026, 4, 14, 9, 0, 0),
                new DateTime(2026, 4, 14, 9, 30, 0));
            var tenantContext = CreateTenantContext(seedData.User.Id, seedData.Tenant.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = CreateCommandService(context, tenantContext);

            var updated = await commandService.RescheduleAsync(
                appointment.Id,
                new RescheduleAppointmentCommand(
                    new DateTime(2026, 4, 14, 10, 0, 0),
                    new DateTime(2026, 4, 14, 10, 45, 0)));

            Assert.NotNull(updated);
            Assert.Equal(new DateTime(2026, 4, 14, 10, 0, 0), updated!.StartsAt);
            Assert.Equal(new DateTime(2026, 4, 14, 10, 45, 0), updated.EndsAt);

            await using var verificationContext = CreateContext(databaseName, tenantContext);
            var storedAppointment = await verificationContext.Appointments.SingleAsync();

            Assert.Equal(new DateTime(2026, 4, 14, 10, 0, 0), storedAppointment.StartsAt);
            Assert.Equal(new DateTime(2026, 4, 14, 10, 45, 0), storedAppointment.EndsAt);
        }

        [Fact]
        public async Task MarkAttendedAsync_UpdatesStoredStatus_WithinTenantScope()
        {
            var databaseName = Guid.NewGuid().ToString();
            var seedData = await SeedTenantWithUserAsync(databaseName);
            var patient = await SeedPatientAsync(databaseName, seedData.Tenant.Id, "Ana", "Lopez");
            var appointment = await SeedAppointmentAsync(
                databaseName,
                seedData.Tenant.Id,
                seedData.PrimaryBranch.Id,
                patient.Id,
                new DateTime(2026, 4, 14, 9, 0, 0),
                new DateTime(2026, 4, 14, 9, 30, 0));
            var tenantContext = CreateTenantContext(seedData.User.Id, seedData.Tenant.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = CreateCommandService(context, tenantContext);

            var updated = await commandService.MarkAttendedAsync(appointment.Id);

            Assert.NotNull(updated);
            Assert.Equal("Attended", updated!.Status);

            await using var verificationContext = CreateContext(databaseName, tenantContext);
            var storedAppointment = await verificationContext.Appointments.SingleAsync();

            Assert.Equal(AppointmentStatus.Attended, storedAppointment.Status);
        }

        [Fact]
        public async Task MarkNoShowAsync_UpdatesStoredStatus_WithinTenantScope()
        {
            var databaseName = Guid.NewGuid().ToString();
            var seedData = await SeedTenantWithUserAsync(databaseName);
            var patient = await SeedPatientAsync(databaseName, seedData.Tenant.Id, "Ana", "Lopez");
            var appointment = await SeedAppointmentAsync(
                databaseName,
                seedData.Tenant.Id,
                seedData.PrimaryBranch.Id,
                patient.Id,
                new DateTime(2026, 4, 14, 10, 0, 0),
                new DateTime(2026, 4, 14, 10, 30, 0));
            var tenantContext = CreateTenantContext(seedData.User.Id, seedData.Tenant.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = CreateCommandService(context, tenantContext);

            var updated = await commandService.MarkNoShowAsync(appointment.Id);

            Assert.NotNull(updated);
            Assert.Equal("NoShow", updated!.Status);

            await using var verificationContext = CreateContext(databaseName, tenantContext);
            var storedAppointment = await verificationContext.Appointments.SingleAsync();

            Assert.Equal(AppointmentStatus.NoShow, storedAppointment.Status);
        }

        [Fact]
        public async Task ChangeConfirmationAsync_ConfirmsAppointmentInsideTenantAndBranchScope()
        {
            var databaseName = Guid.NewGuid().ToString();
            var seedData = await SeedTenantWithUserAsync(databaseName);
            var patient = await SeedPatientAsync(databaseName, seedData.Tenant.Id, "Ana", "Lopez");
            var appointment = await SeedAppointmentAsync(
                databaseName,
                seedData.Tenant.Id,
                seedData.PrimaryBranch.Id,
                patient.Id,
                new DateTime(2026, 4, 14, 9, 0, 0),
                new DateTime(2026, 4, 14, 9, 30, 0));
            var tenantContext = CreateTenantContext(seedData.User.Id, seedData.Tenant.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = CreateCommandService(context, tenantContext);

            var updated = await commandService.ChangeConfirmationAsync(
                appointment.Id,
                new ChangeAppointmentConfirmationCommand("Confirmed"));

            Assert.NotNull(updated);
            Assert.Equal("Confirmed", updated!.ConfirmationStatus);
            Assert.NotNull(updated.ConfirmedAtUtc);
            Assert.Equal(seedData.User.Id, updated.ConfirmedByUserId);
            Assert.Equal("Scheduled", updated.Status);

            await using var verificationContext = CreateContext(databaseName, tenantContext);
            var storedAppointment = await verificationContext.Appointments.SingleAsync();

            Assert.Equal(AppointmentConfirmationStatus.Confirmed, storedAppointment.ConfirmationStatus);
            Assert.NotNull(storedAppointment.ConfirmedAtUtc);
            Assert.Equal(seedData.User.Id, storedAppointment.ConfirmedByUserId);
            Assert.Equal(AppointmentStatus.Scheduled, storedAppointment.Status);
            Assert.Equal(seedData.Tenant.Id, storedAppointment.TenantId);
            Assert.Equal(seedData.PrimaryBranch.Id, storedAppointment.BranchId);
            Assert.Equal(patient.Id, storedAppointment.PatientId);
        }

        [Fact]
        public async Task ChangeConfirmationAsync_MarksConfirmedAppointmentPendingAndClearsMetadata()
        {
            var databaseName = Guid.NewGuid().ToString();
            var seedData = await SeedTenantWithUserAsync(databaseName);
            var patient = await SeedPatientAsync(databaseName, seedData.Tenant.Id, "Ana", "Lopez");
            var appointment = await SeedAppointmentAsync(
                databaseName,
                seedData.Tenant.Id,
                seedData.PrimaryBranch.Id,
                patient.Id,
                new DateTime(2026, 4, 14, 9, 0, 0),
                new DateTime(2026, 4, 14, 9, 30, 0));
            var tenantContext = CreateTenantContext(seedData.User.Id, seedData.Tenant.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = CreateCommandService(context, tenantContext);

            await commandService.ChangeConfirmationAsync(
                appointment.Id,
                new ChangeAppointmentConfirmationCommand("Confirmed"));
            var updated = await commandService.ChangeConfirmationAsync(
                appointment.Id,
                new ChangeAppointmentConfirmationCommand("Pending"));

            Assert.NotNull(updated);
            Assert.Equal("Pending", updated!.ConfirmationStatus);
            Assert.Null(updated.ConfirmedAtUtc);
            Assert.Null(updated.ConfirmedByUserId);
            Assert.Equal("Scheduled", updated.Status);

            await using var verificationContext = CreateContext(databaseName, tenantContext);
            var storedAppointment = await verificationContext.Appointments.SingleAsync();

            Assert.Equal(AppointmentConfirmationStatus.Pending, storedAppointment.ConfirmationStatus);
            Assert.Null(storedAppointment.ConfirmedAtUtc);
            Assert.Null(storedAppointment.ConfirmedByUserId);
            Assert.Equal(AppointmentStatus.Scheduled, storedAppointment.Status);
            Assert.Equal(seedData.Tenant.Id, storedAppointment.TenantId);
            Assert.Equal(seedData.PrimaryBranch.Id, storedAppointment.BranchId);
            Assert.Equal(patient.Id, storedAppointment.PatientId);
        }

        [Fact]
        public async Task ChangeConfirmationAsync_ReturnsNullForAppointmentOutsideCurrentTenant()
        {
            var databaseName = Guid.NewGuid().ToString();
            var tenantASeed = await SeedTenantWithUserAsync(databaseName, tenantName: "Tenant A", tenantSubdomain: "tenant-a");
            var tenantBSeed = await SeedTenantWithUserAsync(databaseName, tenantName: "Tenant B", tenantSubdomain: "tenant-b");
            var patientB = await SeedPatientAsync(databaseName, tenantBSeed.Tenant.Id, "Bruno", "Garcia");
            var foreignAppointment = await SeedAppointmentAsync(
                databaseName,
                tenantBSeed.Tenant.Id,
                tenantBSeed.PrimaryBranch.Id,
                patientB.Id,
                new DateTime(2026, 4, 14, 9, 0, 0),
                new DateTime(2026, 4, 14, 9, 30, 0));
            var tenantContext = CreateTenantContext(tenantASeed.User.Id, tenantASeed.Tenant.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = CreateCommandService(context, tenantContext);

            var updated = await commandService.ChangeConfirmationAsync(
                foreignAppointment.Id,
                new ChangeAppointmentConfirmationCommand("Confirmed"));

            Assert.Null(updated);
        }

        [Fact]
        public async Task ChangeConfirmationAsync_BlocksBranchOutsideAssignedScope_ForTenantUser()
        {
            var databaseName = Guid.NewGuid().ToString();
            var seedData = await SeedTenantWithUserAsync(databaseName);
            var patient = await SeedPatientAsync(databaseName, seedData.Tenant.Id, "Ana", "Lopez");
            var appointment = await SeedAppointmentAsync(
                databaseName,
                seedData.Tenant.Id,
                seedData.SecondaryBranch.Id,
                patient.Id,
                new DateTime(2026, 4, 14, 11, 0, 0),
                new DateTime(2026, 4, 14, 11, 30, 0));
            var tenantContext = CreateTenantContext(seedData.User.Id, seedData.Tenant.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = CreateCommandService(context, tenantContext);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => commandService.ChangeConfirmationAsync(
                appointment.Id,
                new ChangeAppointmentConfirmationCommand("Confirmed")));

            Assert.Contains("branch is not accessible", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task MarkAttendedAsync_BlocksCancelledAppointments()
        {
            var databaseName = Guid.NewGuid().ToString();
            var seedData = await SeedTenantWithUserAsync(databaseName);
            var patient = await SeedPatientAsync(databaseName, seedData.Tenant.Id, "Ana", "Lopez");
            var appointment = await SeedAppointmentAsync(
                databaseName,
                seedData.Tenant.Id,
                seedData.PrimaryBranch.Id,
                patient.Id,
                new DateTime(2026, 4, 14, 9, 0, 0),
                new DateTime(2026, 4, 14, 9, 30, 0),
                status: AppointmentStatus.Cancelled);
            var tenantContext = CreateTenantContext(seedData.User.Id, seedData.Tenant.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = CreateCommandService(context, tenantContext);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => commandService.MarkAttendedAsync(appointment.Id));

            Assert.Contains("Cancelled appointments cannot be marked as attended", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task MarkNoShowAsync_BlocksAttendedAppointments()
        {
            var databaseName = Guid.NewGuid().ToString();
            var seedData = await SeedTenantWithUserAsync(databaseName);
            var patient = await SeedPatientAsync(databaseName, seedData.Tenant.Id, "Ana", "Lopez");
            var appointment = await SeedAppointmentAsync(
                databaseName,
                seedData.Tenant.Id,
                seedData.PrimaryBranch.Id,
                patient.Id,
                new DateTime(2026, 4, 14, 9, 0, 0),
                new DateTime(2026, 4, 14, 9, 30, 0),
                status: AppointmentStatus.Attended);
            var tenantContext = CreateTenantContext(seedData.User.Id, seedData.Tenant.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = CreateCommandService(context, tenantContext);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => commandService.MarkNoShowAsync(appointment.Id));

            Assert.Contains("Attended appointments cannot be marked as no-show", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task CancelAsync_BlocksAttendedAppointments()
        {
            var databaseName = Guid.NewGuid().ToString();
            var seedData = await SeedTenantWithUserAsync(databaseName);
            var patient = await SeedPatientAsync(databaseName, seedData.Tenant.Id, "Ana", "Lopez");
            var appointment = await SeedAppointmentAsync(
                databaseName,
                seedData.Tenant.Id,
                seedData.PrimaryBranch.Id,
                patient.Id,
                new DateTime(2026, 4, 14, 9, 0, 0),
                new DateTime(2026, 4, 14, 9, 30, 0),
                status: AppointmentStatus.Attended);
            var tenantContext = CreateTenantContext(seedData.User.Id, seedData.Tenant.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = CreateCommandService(context, tenantContext);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => commandService.CancelAsync(
                appointment.Id,
                new CancelAppointmentCommand(null)));

            Assert.Contains("Attended appointments cannot be cancelled", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task MarkNoShowAsync_BlocksBranchOutsideAssignedScope_ForTenantUser()
        {
            var databaseName = Guid.NewGuid().ToString();
            var seedData = await SeedTenantWithUserAsync(databaseName);
            var patient = await SeedPatientAsync(databaseName, seedData.Tenant.Id, "Ana", "Lopez");
            var appointment = await SeedAppointmentAsync(
                databaseName,
                seedData.Tenant.Id,
                seedData.SecondaryBranch.Id,
                patient.Id,
                new DateTime(2026, 4, 14, 11, 0, 0),
                new DateTime(2026, 4, 14, 11, 30, 0));
            var tenantContext = CreateTenantContext(seedData.User.Id, seedData.Tenant.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = CreateCommandService(context, tenantContext);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => commandService.MarkNoShowAsync(appointment.Id));

            Assert.Contains("branch is not accessible", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task MarkAttendedAsync_ReturnsNullForAppointmentOutsideCurrentTenant()
        {
            var databaseName = Guid.NewGuid().ToString();
            var tenantASeed = await SeedTenantWithUserAsync(databaseName, tenantName: "Tenant A", tenantSubdomain: "tenant-a");
            var tenantBSeed = await SeedTenantWithUserAsync(databaseName, tenantName: "Tenant B", tenantSubdomain: "tenant-b");
            var patientB = await SeedPatientAsync(databaseName, tenantBSeed.Tenant.Id, "Bruno", "Garcia");
            var foreignAppointment = await SeedAppointmentAsync(
                databaseName,
                tenantBSeed.Tenant.Id,
                tenantBSeed.PrimaryBranch.Id,
                patientB.Id,
                new DateTime(2026, 4, 14, 9, 0, 0),
                new DateTime(2026, 4, 14, 9, 30, 0));
            var tenantContext = CreateTenantContext(tenantASeed.User.Id, tenantASeed.Tenant.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = CreateCommandService(context, tenantContext);

            var updated = await commandService.MarkAttendedAsync(foreignAppointment.Id);

            Assert.Null(updated);
        }

        [Fact]
        public async Task GetCalendarAsync_ExposesAttendedAndNoShowStatuses()
        {
            var databaseName = Guid.NewGuid().ToString();
            var seedData = await SeedTenantWithUserAsync(databaseName);
            var patientA = await SeedPatientAsync(databaseName, seedData.Tenant.Id, "Ana", "Lopez");
            var patientB = await SeedPatientAsync(databaseName, seedData.Tenant.Id, "Bruno", "Garcia");

            await SeedAppointmentAsync(
                databaseName,
                seedData.Tenant.Id,
                seedData.PrimaryBranch.Id,
                patientA.Id,
                new DateTime(2026, 4, 14, 9, 0, 0),
                new DateTime(2026, 4, 14, 9, 30, 0),
                status: AppointmentStatus.Attended);
            await SeedAppointmentAsync(
                databaseName,
                seedData.Tenant.Id,
                seedData.PrimaryBranch.Id,
                patientB.Id,
                new DateTime(2026, 4, 14, 10, 0, 0),
                new DateTime(2026, 4, 14, 10, 30, 0),
                status: AppointmentStatus.NoShow);

            var tenantContext = CreateTenantContext(seedData.User.Id, seedData.Tenant.Id);
            await using var context = CreateContext(databaseName, tenantContext);
            var queryService = CreateQueryService(context, tenantContext);

            var calendar = await queryService.GetCalendarAsync(
                seedData.PrimaryBranch.Id,
                new DateOnly(2026, 4, 14),
                1);

            Assert.Equal(new[] { "Attended", "NoShow" }, calendar.CalendarDays[0].Appointments.Select(appointment => appointment.Status));
        }

        [Fact]
        public async Task GetCalendarAsync_ExposesAppointmentConfirmationFields()
        {
            var databaseName = Guid.NewGuid().ToString();
            var seedData = await SeedTenantWithUserAsync(databaseName);
            var patient = await SeedPatientAsync(databaseName, seedData.Tenant.Id, "Ana", "Lopez");
            var appointment = await SeedAppointmentAsync(
                databaseName,
                seedData.Tenant.Id,
                seedData.PrimaryBranch.Id,
                patient.Id,
                new DateTime(2026, 4, 14, 9, 0, 0),
                new DateTime(2026, 4, 14, 9, 30, 0));
            var tenantContext = CreateTenantContext(seedData.User.Id, seedData.Tenant.Id);

            await using var commandContext = CreateContext(databaseName, tenantContext);
            var commandService = CreateCommandService(commandContext, tenantContext);
            await commandService.ChangeConfirmationAsync(
                appointment.Id,
                new ChangeAppointmentConfirmationCommand("Confirmed"));

            await using var queryContext = CreateContext(databaseName, tenantContext);
            var queryService = CreateQueryService(queryContext, tenantContext);

            var calendar = await queryService.GetCalendarAsync(
                seedData.PrimaryBranch.Id,
                new DateOnly(2026, 4, 14),
                1);

            var calendarAppointment = Assert.Single(calendar.CalendarDays[0].Appointments);
            Assert.Equal("Confirmed", calendarAppointment.ConfirmationStatus);
            Assert.NotNull(calendarAppointment.ConfirmedAtUtc);
            Assert.Equal(seedData.User.Id, calendarAppointment.ConfirmedByUserId);
        }

        [Fact]
        public async Task CreateAsync_BlocksSchedulingWhenBlockedSlotExists()
        {
            var databaseName = Guid.NewGuid().ToString();
            var seedData = await SeedTenantWithUserAsync(databaseName);
            var patient = await SeedPatientAsync(databaseName, seedData.Tenant.Id, "Ana", "Lopez");
            await SeedAppointmentBlockAsync(
                databaseName,
                seedData.Tenant.Id,
                seedData.PrimaryBranch.Id,
                new DateTime(2026, 4, 14, 9, 0, 0),
                new DateTime(2026, 4, 14, 10, 0, 0),
                "Morning huddle");
            var tenantContext = CreateTenantContext(seedData.User.Id, seedData.Tenant.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = CreateCommandService(context, tenantContext);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => commandService.CreateAsync(
                new CreateAppointmentCommand(
                    seedData.PrimaryBranch.Id,
                    patient.Id,
                    new DateTime(2026, 4, 14, 9, 15, 0),
                    new DateTime(2026, 4, 14, 9, 45, 0),
                    null)));

            Assert.Contains("blocked time slots", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task RescheduleAsync_BlocksSchedulingWhenBlockedSlotExists()
        {
            var databaseName = Guid.NewGuid().ToString();
            var seedData = await SeedTenantWithUserAsync(databaseName);
            var patient = await SeedPatientAsync(databaseName, seedData.Tenant.Id, "Ana", "Lopez");
            var appointment = await SeedAppointmentAsync(
                databaseName,
                seedData.Tenant.Id,
                seedData.PrimaryBranch.Id,
                patient.Id,
                new DateTime(2026, 4, 14, 8, 0, 0),
                new DateTime(2026, 4, 14, 8, 30, 0));
            await SeedAppointmentBlockAsync(
                databaseName,
                seedData.Tenant.Id,
                seedData.PrimaryBranch.Id,
                new DateTime(2026, 4, 14, 9, 0, 0),
                new DateTime(2026, 4, 14, 10, 0, 0),
                "Morning huddle");
            var tenantContext = CreateTenantContext(seedData.User.Id, seedData.Tenant.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = CreateCommandService(context, tenantContext);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => commandService.RescheduleAsync(
                appointment.Id,
                new RescheduleAppointmentCommand(
                    new DateTime(2026, 4, 14, 9, 15, 0),
                    new DateTime(2026, 4, 14, 9, 45, 0))));

            Assert.Contains("blocked time slots", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        private static AppointmentCommandService CreateCommandService(AppDbContext context, TenantContext tenantContext)
        {
            return new AppointmentCommandService(
                new EfAppointmentRepository(context),
                new EfAppointmentBlockRepository(context),
                new EfPatientRepository(context),
                new BranchAccessService(
                    new EfBranchRepository(context),
                    new EfUserTenantMembershipRepository(context),
                    new RolePermissionCatalog(),
                    tenantContext),
                tenantContext);
        }

        private static AppointmentQueryService CreateQueryService(AppDbContext context, TenantContext tenantContext)
        {
            return new AppointmentQueryService(
                new EfAppointmentRepository(context),
                new EfAppointmentBlockRepository(context),
                new BranchAccessService(
                    new EfBranchRepository(context),
                    new EfUserTenantMembershipRepository(context),
                    new RolePermissionCatalog(),
                    tenantContext),
                tenantContext);
        }

        private static TenantContext CreateTenantContext(Guid userId, Guid tenantId)
        {
            var tenantContext = new TenantContext();
            tenantContext.SetRequestContext(userId.ToString(), AccessScope.Tenant, isAuthenticated: true, tenantId.ToString());
            return tenantContext;
        }

        private static async Task<SeedData> SeedTenantWithUserAsync(
            string databaseName,
            string tenantName = "Tenant A",
            string tenantSubdomain = "tenant-a")
        {
            await using var context = CreateContext(databaseName, new TenantContext());
            var tenant = new Tenant(tenantName, tenantSubdomain);
            var primaryBranch = tenant.AddBranch($"{tenantName} Main");
            var secondaryBranch = tenant.AddBranch($"{tenantName} Secondary");
            var role = new Role(SystemRoles.TenantUser);
            var user = new User($"{tenantSubdomain}@example.com", "hashed-password", $"{tenantName} User");
            var membership = user.AddTenantMembership(tenant, role);
            membership.AssignToBranch(primaryBranch);

            context.Tenants.Add(tenant);
            context.Roles.Add(role);
            context.Users.Add(user);
            context.UserTenantMemberships.Add(membership);
            await context.SaveChangesAsync();

            return new SeedData(tenant, user, primaryBranch, secondaryBranch);
        }

        private static async Task<Patient> SeedPatientAsync(string databaseName, Guid tenantId, string firstName, string lastName)
        {
            await using var context = CreateContext(databaseName, new TenantContext());
            var patient = new Patient(
                tenantId,
                firstName,
                lastName,
                new DateOnly(1991, 2, 14),
                "5551234567",
                $"{firstName.ToLowerInvariant()}@example.com");

            context.Patients.Add(patient);
            await context.SaveChangesAsync();

            return patient;
        }

        private static async Task<Appointment> SeedAppointmentAsync(
            string databaseName,
            Guid tenantId,
            Guid branchId,
            Guid patientId,
            DateTime startsAt,
            DateTime endsAt,
            AppointmentStatus status = AppointmentStatus.Scheduled)
        {
            await using var context = CreateContext(databaseName, new TenantContext());
            var appointment = new Appointment(tenantId, branchId, patientId, startsAt, endsAt, "Follow-up");

            switch (status)
            {
                case AppointmentStatus.Cancelled:
                    appointment.Cancel("Cancelled during test setup.");
                    break;
                case AppointmentStatus.Attended:
                    appointment.MarkAttended();
                    break;
                case AppointmentStatus.NoShow:
                    appointment.MarkNoShow();
                    break;
            }

            context.Appointments.Add(appointment);
            await context.SaveChangesAsync();

            return appointment;
        }

        private static async Task<AppointmentBlock> SeedAppointmentBlockAsync(
            string databaseName,
            Guid tenantId,
            Guid branchId,
            DateTime startsAt,
            DateTime endsAt,
            string? label)
        {
            await using var context = CreateContext(databaseName, new TenantContext());
            var appointmentBlock = new AppointmentBlock(tenantId, branchId, startsAt, endsAt, label);

            context.AppointmentBlocks.Add(appointmentBlock);
            await context.SaveChangesAsync();

            return appointmentBlock;
        }

        private static AppDbContext CreateContext(string databaseName, TenantContext tenantContext)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName)
                .Options;

            return new AppDbContext(options, CreateConfiguration(), tenantContext);
        }

        private static IConfiguration CreateConfiguration()
        {
            return new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>())
                .Build();
        }

        private sealed record SeedData(
            Tenant Tenant,
            User User,
            Branch PrimaryBranch,
            Branch SecondaryBranch);
    }
}
