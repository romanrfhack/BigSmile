using BigSmile.Domain.Entities;

namespace BigSmile.UnitTests.Scheduling
{
    public class AppointmentTests
    {
        [Fact]
        public void NewAppointment_StartsWithPendingConfirmation()
        {
            var appointment = CreateAppointment();

            Assert.Equal(AppointmentConfirmationStatus.Pending, appointment.ConfirmationStatus);
            Assert.Null(appointment.ConfirmedAtUtc);
            Assert.Null(appointment.ConfirmedByUserId);
        }

        [Fact]
        public void Confirm_SetsConfirmationMetadataWithoutChangingOwnershipOrAppointmentStatus()
        {
            var tenantId = Guid.NewGuid();
            var branchId = Guid.NewGuid();
            var patientId = Guid.NewGuid();
            var actorUserId = Guid.NewGuid();
            var appointment = CreateAppointment(tenantId, branchId, patientId);
            var beforeConfirmation = DateTime.UtcNow;

            appointment.Confirm(actorUserId);

            Assert.Equal(AppointmentConfirmationStatus.Confirmed, appointment.ConfirmationStatus);
            Assert.NotNull(appointment.ConfirmedAtUtc);
            Assert.True(appointment.ConfirmedAtUtc >= beforeConfirmation);
            Assert.Equal(actorUserId, appointment.ConfirmedByUserId);
            Assert.Equal(tenantId, appointment.TenantId);
            Assert.Equal(branchId, appointment.BranchId);
            Assert.Equal(patientId, appointment.PatientId);
            Assert.Equal(AppointmentStatus.Scheduled, appointment.Status);
        }

        [Fact]
        public void MarkConfirmationPending_ClearsConfirmationMetadataWithoutChangingOwnershipOrAppointmentStatus()
        {
            var tenantId = Guid.NewGuid();
            var branchId = Guid.NewGuid();
            var patientId = Guid.NewGuid();
            var actorUserId = Guid.NewGuid();
            var appointment = CreateAppointment(tenantId, branchId, patientId);
            appointment.Confirm(actorUserId);

            appointment.MarkConfirmationPending(actorUserId);

            Assert.Equal(AppointmentConfirmationStatus.Pending, appointment.ConfirmationStatus);
            Assert.Null(appointment.ConfirmedAtUtc);
            Assert.Null(appointment.ConfirmedByUserId);
            Assert.Equal(tenantId, appointment.TenantId);
            Assert.Equal(branchId, appointment.BranchId);
            Assert.Equal(patientId, appointment.PatientId);
            Assert.Equal(AppointmentStatus.Scheduled, appointment.Status);
        }

        [Theory]
        [InlineData(AppointmentStatus.Cancelled)]
        [InlineData(AppointmentStatus.Attended)]
        [InlineData(AppointmentStatus.NoShow)]
        public void Confirm_BlocksTerminalAppointmentStatuses(AppointmentStatus terminalStatus)
        {
            var appointment = CreateAppointment();
            MoveToStatus(appointment, terminalStatus);

            var exception = Assert.Throws<InvalidOperationException>(() => appointment.Confirm(Guid.NewGuid()));

            Assert.Contains("cannot have confirmation changed", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Theory]
        [InlineData(AppointmentStatus.Cancelled)]
        [InlineData(AppointmentStatus.Attended)]
        [InlineData(AppointmentStatus.NoShow)]
        public void MarkConfirmationPending_BlocksTerminalAppointmentStatuses(AppointmentStatus terminalStatus)
        {
            var actorUserId = Guid.NewGuid();
            var appointment = CreateAppointment();
            appointment.Confirm(actorUserId);
            MoveToStatus(appointment, terminalStatus);

            var exception = Assert.Throws<InvalidOperationException>(() => appointment.MarkConfirmationPending(actorUserId));

            Assert.Contains("cannot have confirmation changed", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Confirm_RequiresActor()
        {
            var appointment = CreateAppointment();

            Assert.Throws<ArgumentException>(() => appointment.Confirm(Guid.Empty));
        }

        [Fact]
        public void AddReminderLogEntry_CreatesManualEntryWithoutChangingAppointmentOrConfirmationStatus()
        {
            var tenantId = Guid.NewGuid();
            var branchId = Guid.NewGuid();
            var patientId = Guid.NewGuid();
            var actorUserId = Guid.NewGuid();
            var appointment = CreateAppointment(tenantId, branchId, patientId);

            var entry = appointment.AddReminderLogEntry(
                AppointmentReminderChannel.Phone,
                AppointmentReminderOutcome.Reached,
                " Confirmed by phone. ",
                actorUserId);

            Assert.Equal(tenantId, entry.TenantId);
            Assert.Equal(appointment.Id, entry.AppointmentId);
            Assert.Equal(AppointmentReminderChannel.Phone, entry.Channel);
            Assert.Equal(AppointmentReminderOutcome.Reached, entry.Outcome);
            Assert.Equal("Confirmed by phone.", entry.Notes);
            Assert.Equal(actorUserId, entry.CreatedByUserId);
            Assert.Equal(AppointmentStatus.Scheduled, appointment.Status);
            Assert.Equal(AppointmentConfirmationStatus.Pending, appointment.ConfirmationStatus);
            Assert.Null(appointment.ConfirmedAtUtc);
            Assert.Null(appointment.ConfirmedByUserId);
        }

        [Fact]
        public void AddReminderLogEntry_AllowsOptionalNotes()
        {
            var appointment = CreateAppointment();

            var entry = appointment.AddReminderLogEntry(
                AppointmentReminderChannel.Email,
                AppointmentReminderOutcome.LeftMessage,
                "   ",
                Guid.NewGuid());

            Assert.Null(entry.Notes);
        }

        [Fact]
        public void AddReminderLogEntry_BlocksNotesOverLimit()
        {
            var appointment = CreateAppointment();
            var notes = new string('a', AppointmentReminderLogEntry.NotesMaxLength + 1);

            Assert.Throws<ArgumentException>(() => appointment.AddReminderLogEntry(
                AppointmentReminderChannel.Other,
                AppointmentReminderOutcome.NoAnswer,
                notes,
                Guid.NewGuid()));
        }

        [Fact]
        public void AppointmentReminderLogEntry_BlocksInvalidChannel()
        {
            var appointmentId = Guid.NewGuid();

            Assert.Throws<ArgumentException>(() => new AppointmentReminderLogEntry(
                Guid.NewGuid(),
                appointmentId,
                (AppointmentReminderChannel)999,
                AppointmentReminderOutcome.Reached,
                null,
                Guid.NewGuid()));
        }

        [Fact]
        public void AppointmentReminderLogEntry_BlocksInvalidOutcome()
        {
            var appointmentId = Guid.NewGuid();

            Assert.Throws<ArgumentException>(() => new AppointmentReminderLogEntry(
                Guid.NewGuid(),
                appointmentId,
                AppointmentReminderChannel.Phone,
                (AppointmentReminderOutcome)999,
                null,
                Guid.NewGuid()));
        }

        [Theory]
        [InlineData(AppointmentStatus.Cancelled)]
        [InlineData(AppointmentStatus.Attended)]
        [InlineData(AppointmentStatus.NoShow)]
        public void AddReminderLogEntry_AllowsTerminalAppointmentStatuses(AppointmentStatus terminalStatus)
        {
            var appointment = CreateAppointment();
            MoveToStatus(appointment, terminalStatus);

            var entry = appointment.AddReminderLogEntry(
                AppointmentReminderChannel.WhatsApp,
                AppointmentReminderOutcome.NoAnswer,
                null,
                Guid.NewGuid());

            Assert.Equal(terminalStatus, appointment.Status);
            Assert.Equal(AppointmentConfirmationStatus.Pending, appointment.ConfirmationStatus);
            Assert.Equal(AppointmentReminderChannel.WhatsApp, entry.Channel);
        }

        private static Appointment CreateAppointment(
            Guid? tenantId = null,
            Guid? branchId = null,
            Guid? patientId = null)
        {
            return new Appointment(
                tenantId ?? Guid.NewGuid(),
                branchId ?? Guid.NewGuid(),
                patientId ?? Guid.NewGuid(),
                new DateTime(2026, 4, 25, 9, 0, 0),
                new DateTime(2026, 4, 25, 9, 30, 0),
                "Follow-up");
        }

        private static void MoveToStatus(Appointment appointment, AppointmentStatus status)
        {
            switch (status)
            {
                case AppointmentStatus.Cancelled:
                    appointment.Cancel("Cancelled in test.");
                    break;
                case AppointmentStatus.Attended:
                    appointment.MarkAttended();
                    break;
                case AppointmentStatus.NoShow:
                    appointment.MarkNoShow();
                    break;
            }
        }
    }
}
