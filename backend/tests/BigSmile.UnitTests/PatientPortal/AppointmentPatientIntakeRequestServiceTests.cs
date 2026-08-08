using BigSmile.Application.Features.Branches.Services;
using BigSmile.Application.Features.PatientIntakeRequests.Dtos;
using BigSmile.Application.Features.PatientIntakeRequests.Services;
using BigSmile.Application.Features.PatientPortalInvitations.Commands;
using BigSmile.Application.Features.PatientPortalInvitations.Dtos;
using BigSmile.Application.Interfaces.Repositories;
using BigSmile.Domain.Entities;
using BigSmile.SharedKernel.Authorization;
using BigSmile.SharedKernel.Context;
using Moq;

namespace BigSmile.UnitTests.PatientPortal
{
    public sealed class AppointmentPatientIntakeRequestServiceTests
    {
        private static readonly DateTime UtcNow =
            new(2026, 8, 6, 13, 30, 0, DateTimeKind.Utc);

        [Fact]
        public async Task PrepareAsync_WithoutAccount_IssuesActivationForAccessibleAppointment()
        {
            var fixture = CreateFixture();
            fixture.InvitationService
                .Setup(service => service.IssueAsync(
                    fixture.Patient.Id,
                    "request-1",
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new IssuedPatientPortalInvitationDto(
                    Guid.NewGuid(),
                    fixture.Patient.Id,
                    PatientPortalInvitationPurpose.ExistingPatientActivation.ToString(),
                    "raw-activation-token",
                    UtcNow,
                    UtcNow.AddHours(24)));

            var status = await fixture.Service.GetStatusAsync(fixture.Appointment.Id);
            var prepared = await fixture.Service.PrepareAsync(
                fixture.Appointment.Id,
                "request-1");

            Assert.NotNull(status);
            Assert.Equal("NotActivated", status!.PortalAccessStatus);
            Assert.Equal("NotStarted", status.IntakeStatus);
            Assert.Equal("Activation", status.RecommendedAccess);
            Assert.True(status.CanRequest);
            Assert.True(prepared.Succeeded);
            Assert.Equal("Activation", prepared.PreparedRequest!.AccessMode);
            Assert.Equal("raw-activation-token", prepared.PreparedRequest.ActivationToken);
            Assert.Equal("tenant-a", prepared.PreparedRequest.Status.PatientPortalRealm);
        }

        [Fact]
        public async Task ActiveAccount_UsesLoginAndShowsDraftInProgressWithoutIssuingInvitation()
        {
            var fixture = CreateFixture();
            var account = PatientPortalAccount.CreateForExistingPatient(
                fixture.Patient,
                "ana.portal",
                "versioned-hash",
                UtcNow.AddDays(-1));
            var draft = PatientIntake.CreateForExistingPatient(
                account,
                fixture.Patient,
                fixture.Branch,
                UtcNow.AddHours(-1));
            fixture.PortalRepository
                .Setup(repository => repository.GetAccountByPatientAsync(
                    fixture.Tenant.Id,
                    fixture.Patient.Id,
                    false,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(account);
            fixture.IntakeRepository
                .Setup(repository => repository.GetDraftByAccountAsync(
                    fixture.Tenant.Id,
                    account.Id,
                    false,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(draft);

            var prepared = await fixture.Service.PrepareAsync(
                fixture.Appointment.Id,
                "request-login");

            Assert.True(prepared.Succeeded);
            Assert.Equal("Active", prepared.PreparedRequest!.Status.PortalAccessStatus);
            Assert.Equal("InProgress", prepared.PreparedRequest.Status.IntakeStatus);
            Assert.Equal("Login", prepared.PreparedRequest.AccessMode);
            Assert.Null(prepared.PreparedRequest.ActivationToken);
            fixture.InvitationService.Verify(
                service => service.IssueAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task SubmittedHistory_IsCompletedAndCannotPrepareAnotherLink()
        {
            var fixture = CreateFixture();
            var account = PatientPortalAccount.CreateForExistingPatient(
                fixture.Patient,
                "ana.portal",
                "versioned-hash",
                UtcNow.AddDays(-1));
            var submitted = CreateSubmittedIntake(
                account,
                fixture.Patient,
                fixture.Branch);
            fixture.PortalRepository
                .Setup(repository => repository.GetAccountByPatientAsync(
                    fixture.Tenant.Id,
                    fixture.Patient.Id,
                    false,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(account);
            fixture.IntakeRepository
                .Setup(repository => repository.GetSubmittedByPatientAsync(
                    fixture.Tenant.Id,
                    fixture.Patient.Id,
                    false,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(submitted);

            var status = await fixture.Service.GetStatusAsync(fixture.Appointment.Id);
            var prepared = await fixture.Service.PrepareAsync(
                fixture.Appointment.Id,
                "request-after-completion");

            Assert.NotNull(status);
            Assert.Equal("Completed", status!.IntakeStatus);
            Assert.Equal("None", status.RecommendedAccess);
            Assert.False(status.CanRequest);
            Assert.Equal(submitted.SubmittedAtUtc, status.SubmittedAtUtc);
            Assert.Equal(
                AppointmentPatientIntakeRequestFailure.AlreadyCompleted,
                prepared.Failure);
            fixture.InvitationService.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task InactivePortalAccount_RequiresAssistedRecovery()
        {
            var fixture = CreateFixture();
            var account = PatientPortalAccount.CreateForExistingPatient(
                fixture.Patient,
                "ana.portal",
                "versioned-hash",
                UtcNow.AddDays(-1));
            account.Deactivate(UtcNow.AddMinutes(-1));
            fixture.PortalRepository
                .Setup(repository => repository.GetAccountByPatientAsync(
                    fixture.Tenant.Id,
                    fixture.Patient.Id,
                    false,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(account);

            var status = await fixture.Service.GetStatusAsync(fixture.Appointment.Id);
            var prepared = await fixture.Service.PrepareAsync(
                fixture.Appointment.Id,
                "request-recovery");

            Assert.NotNull(status);
            Assert.Equal("RecoveryRequired", status!.PortalAccessStatus);
            Assert.Equal("RecoveryRequired", status.RecommendedAccess);
            Assert.False(status.CanRequest);
            Assert.Equal(
                AppointmentPatientIntakeRequestFailure.RecoveryRequired,
                prepared.Failure);
        }

        [Fact]
        public async Task InaccessibleBranch_IsIndistinguishableFromMissingAppointment()
        {
            var fixture = CreateFixture(branchAccessible: false);

            var status = await fixture.Service.GetStatusAsync(fixture.Appointment.Id);
            var prepared = await fixture.Service.PrepareAsync(
                fixture.Appointment.Id,
                "request-outside-branch");

            Assert.Null(status);
            Assert.Equal(AppointmentPatientIntakeRequestFailure.NotFound, prepared.Failure);
            fixture.PortalRepository.VerifyNoOtherCalls();
            fixture.IntakeRepository.VerifyNoOtherCalls();
            fixture.InvitationService.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task NonScheduledAppointment_CannotPreparePatientAccess()
        {
            var fixture = CreateFixture();
            fixture.Appointment.Cancel("Patient cancelled");

            var status = await fixture.Service.GetStatusAsync(fixture.Appointment.Id);
            var prepared = await fixture.Service.PrepareAsync(
                fixture.Appointment.Id,
                "request-cancelled");

            Assert.NotNull(status);
            Assert.False(status!.CanRequest);
            Assert.Equal(AppointmentPatientIntakeRequestFailure.Unavailable, prepared.Failure);
            fixture.InvitationService.VerifyNoOtherCalls();
        }

        private static Fixture CreateFixture(bool branchAccessible = true)
        {
            var tenant = new Tenant("Tenant A", "tenant-a");
            var branch = tenant.AddBranch("Main");
            var patient = new Patient(
                tenant.Id,
                "Ana",
                "Lopez",
                new DateOnly(1991, 2, 14),
                primaryPhone: "+52555550100");
            var appointment = new Appointment(
                tenant.Id,
                branch.Id,
                patient.Id,
                UtcNow.AddDays(2),
                UtcNow.AddDays(2).AddHours(1));
            typeof(Appointment)
                .GetProperty(nameof(Appointment.Patient))!
                .SetValue(appointment, patient);

            var appointmentRepository = new Mock<IAppointmentRepository>();
            appointmentRepository
                .Setup(repository => repository.GetByIdAsync(
                    appointment.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointment);

            var branchAccessService = new Mock<IBranchAccessService>();
            branchAccessService
                .Setup(service => service.GetAccessibleBranchAsync(
                    branch.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(branchAccessible ? branch : null);

            var portalRepository = new Mock<IPatientPortalAuthenticationRepository>();
            var intakeRepository = new Mock<IPatientIntakeRepository>();
            var invitationService = new Mock<IPatientPortalInvitationCommandService>();
            var tenantRepository = new Mock<ITenantRepository>();
            tenantRepository
                .Setup(repository => repository.GetByIdAsync(
                    tenant.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(tenant);

            var tenantContext = new Mock<ITenantContext>();
            tenantContext.Setup(context => context.IsAuthenticated()).Returns(true);
            tenantContext.Setup(context => context.HasPlatformOverride()).Returns(false);
            tenantContext.Setup(context => context.GetAccessScope()).Returns(AccessScope.Tenant);
            tenantContext.Setup(context => context.GetTenantId()).Returns(tenant.Id.ToString());

            var service = new AppointmentPatientIntakeRequestService(
                appointmentRepository.Object,
                branchAccessService.Object,
                portalRepository.Object,
                intakeRepository.Object,
                invitationService.Object,
                tenantRepository.Object,
                tenantContext.Object,
                new FixedTimeProvider(UtcNow));

            return new Fixture(
                tenant,
                branch,
                patient,
                appointment,
                portalRepository,
                intakeRepository,
                invitationService,
                service);
        }

        private static PatientIntake CreateSubmittedIntake(
            PatientPortalAccount account,
            Patient patient,
            Branch branch)
        {
            var intake = PatientIntake.CreateForExistingPatient(
                account,
                patient,
                branch,
                UtcNow.AddHours(-2));
            var completeDraft = intake.GetDraftData() with
            {
                MedicalAnswers = intake.GetDraftData().MedicalAnswers
                    .Select(answer => answer with
                    {
                        Answer = ClinicalMedicalAnswerValue.No
                    })
                    .ToArray()
            };
            intake.SaveDraft(
                completeDraft,
                account.Id,
                UtcNow.AddHours(-1),
                "complete-draft");
            intake.Submit(account.Id, UtcNow.AddMinutes(-30), "submit");
            return intake;
        }

        private sealed record Fixture(
            Tenant Tenant,
            Branch Branch,
            Patient Patient,
            Appointment Appointment,
            Mock<IPatientPortalAuthenticationRepository> PortalRepository,
            Mock<IPatientIntakeRepository> IntakeRepository,
            Mock<IPatientPortalInvitationCommandService> InvitationService,
            AppointmentPatientIntakeRequestService Service);

        private sealed class FixedTimeProvider : TimeProvider
        {
            private readonly DateTimeOffset _utcNow;

            public FixedTimeProvider(DateTime utcNow)
            {
                _utcNow = new DateTimeOffset(utcNow);
            }

            public override DateTimeOffset GetUtcNow() => _utcNow;
        }
    }
}
