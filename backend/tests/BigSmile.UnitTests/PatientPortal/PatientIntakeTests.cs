using System.Text.Json;
using BigSmile.Domain.Entities;

namespace BigSmile.UnitTests.PatientPortal
{
    public sealed class PatientIntakeTests
    {
        private static readonly DateTime CreatedAtUtc =
            new(2026, 7, 25, 12, 0, 0, DateTimeKind.Utc);

        [Fact]
        public void CreateForExistingPatient_PrefillsApprovedPatientFieldsAndBaseline()
        {
            var tenant = new Tenant("Tenant A", "tenant-a");
            var branch = tenant.AddBranch("Main");
            var patient = new Patient(
                tenant.Id,
                "Ana",
                "Lopez",
                new DateOnly(1991, 2, 14),
                primaryPhone: "555-0100",
                email: "ana@example.com",
                responsiblePartyName: "Laura Lopez",
                responsiblePartyRelationship: "Mother",
                responsiblePartyPhone: "555-0101",
                sex: PatientSex.Female,
                occupation: "Designer",
                maritalStatus: PatientMaritalStatus.Single,
                referredBy: "Friend");
            var account = PatientPortalAccount.CreateForExistingPatient(
                patient,
                "ana.portal",
                "versioned-hash");

            var intake = PatientIntake.CreateForExistingPatient(
                account,
                patient,
                branch,
                CreatedAtUtc);

            var draft = intake.GetDraftData();
            Assert.Equal(tenant.Id, intake.TenantId);
            Assert.Equal(account.Id, intake.PatientPortalAccountId);
            Assert.Equal(patient.Id, intake.PatientId);
            Assert.Equal(branch.Id, intake.BranchId);
            Assert.Equal(PatientIntakeOrigin.ExistingPatientPortal, intake.Origin);
            Assert.Equal(PatientIntakeStatus.Draft, intake.Status);
            Assert.Equal(CreatedAtUtc.AddDays(30), intake.ExpiresAtUtc);
            Assert.NotNull(intake.CanonicalPatientBaselineJson);
            Assert.Equal(CreatedAtUtc, intake.CanonicalPatientBaselineCapturedAtUtc);
            Assert.Equal("Ana", draft.FirstName);
            Assert.Equal("Lopez", draft.LastName);
            Assert.Equal(patient.DateOfBirth, draft.DateOfBirth);
            Assert.Equal(PatientSex.Female, draft.Sex);
            Assert.Equal("Designer", draft.Occupation);
            Assert.Equal(PatientMaritalStatus.Single, draft.MaritalStatus);
            Assert.Equal("Friend", draft.ReferredBy);
            Assert.Equal("555-0100", draft.PreferredPhone);
            Assert.Null(draft.MobilePhone);
            Assert.Equal("ana@example.com", draft.Email);
            Assert.Equal("Laura Lopez", draft.ResponsiblePartyName);
            Assert.Equal(39, intake.MedicalAnswers.Count);
            Assert.All(intake.MedicalAnswers, answer =>
                Assert.Equal(ClinicalMedicalAnswerValue.Unknown, answer.Answer));
            Assert.Empty(intake.Revisions);
            Assert.Equal(0, intake.CurrentRevisionNumber);
        }

        [Fact]
        public void CreateForNewPatient_RequiresUnlinkedAccountAndSameTenantBranch()
        {
            var tenant = new Tenant("Tenant A", "tenant-a");
            var branch = tenant.AddBranch("Main");
            var unlinkedAccount = PatientPortalAccount.CreateUnlinked(
                tenant.Id,
                "new.patient",
                "versioned-hash");

            var intake = PatientIntake.CreateForNewPatient(
                unlinkedAccount,
                branch,
                CreatedAtUtc);

            Assert.Equal(PatientIntakeOrigin.NewPatientWaitingRoom, intake.Origin);
            Assert.Null(intake.PatientId);
            Assert.Null(intake.CanonicalPatientBaselineJson);
            Assert.Equal(branch.Id, intake.BranchId);
            Assert.Null(intake.GetDraftData().FirstName);

            var patient = CreatePatient(tenant.Id);
            var linkedAccount = PatientPortalAccount.CreateForExistingPatient(
                patient,
                "linked.patient",
                "versioned-hash");
            Assert.Throws<InvalidOperationException>(() => PatientIntake.CreateForNewPatient(
                linkedAccount,
                branch,
                CreatedAtUtc));

            var foreignBranch = new Tenant("Tenant B", "tenant-b").AddBranch("Foreign");
            Assert.Throws<InvalidOperationException>(() => PatientIntake.CreateForNewPatient(
                unlinkedAccount,
                foreignBranch,
                CreatedAtUtc));
        }

        [Fact]
        public void SaveDraft_NormalizesEffectiveChangeAndCreatesImmutableRevisionSnapshot()
        {
            var account = PatientPortalAccount.CreateUnlinked(
                Guid.NewGuid(),
                "new.patient",
                "versioned-hash");
            var intake = PatientIntake.CreateForNewPatient(
                account,
                branch: null,
                CreatedAtUtc);
            var occurredAtUtc = CreatedAtUtc.AddDays(1);
            var draft = BuildDraft(
                firstName: "  Ana  ",
                reasonForVisit: "  Dolor al masticar  ",
                diabetesAnswer: ClinicalMedicalAnswerValue.Yes,
                diabetesDetails: "  Controlada con dieta  ");

            var revision = intake.SaveDraft(
                draft,
                account.Id,
                occurredAtUtc,
                " correlation-1 ");

            Assert.NotNull(revision);
            Assert.Equal(1, intake.CurrentRevisionNumber);
            Assert.Equal(occurredAtUtc, intake.LastEffectiveSavedAtUtc);
            Assert.Equal(occurredAtUtc.AddDays(30), intake.ExpiresAtUtc);
            Assert.Equal("Ana", intake.FirstName);
            Assert.Equal("Dolor al masticar", intake.ReasonForVisit);
            Assert.Equal("correlation-1", revision!.CorrelationId);
            Assert.Contains("firstName", revision.ChangedFieldsJson, StringComparison.Ordinal);
            Assert.Contains("medicalAnswers.diabetes", revision.ChangedFieldsJson, StringComparison.Ordinal);
            Assert.Contains("reasonForVisit", revision.ChangedFieldsJson, StringComparison.Ordinal);
            Assert.Single(intake.Revisions);

            using var snapshotDocument = JsonDocument.Parse(revision.SnapshotJson);
            Assert.Equal(
                1,
                snapshotDocument.RootElement.GetProperty("schemaVersion").GetInt32());
            var data = snapshotDocument.RootElement.GetProperty("data");
            Assert.Equal("Ana", data.GetProperty("firstName").GetString());
            Assert.Equal("Dolor al masticar", data.GetProperty("reasonForVisit").GetString());
            Assert.Equal(39, data.GetProperty("medicalAnswers").GetArrayLength());
        }

        [Fact]
        public void SaveDraft_IdenticalNormalizedSnapshotDoesNotCreateRevisionOrExtendExpiry()
        {
            var account = PatientPortalAccount.CreateUnlinked(
                Guid.NewGuid(),
                "new.patient",
                "versioned-hash");
            var intake = PatientIntake.CreateForNewPatient(
                account,
                branch: null,
                CreatedAtUtc);
            var draft = BuildDraft(firstName: "Ana");
            var firstSaveAtUtc = CreatedAtUtc.AddHours(1);

            Assert.NotNull(intake.SaveDraft(
                draft,
                account.Id,
                firstSaveAtUtc,
                "correlation-1"));
            var expiryAfterFirstSave = intake.ExpiresAtUtc;

            var identicalResult = intake.SaveDraft(
                BuildDraft(firstName: "  Ana  "),
                account.Id,
                firstSaveAtUtc.AddHours(2),
                "correlation-2");

            Assert.Null(identicalResult);
            Assert.Equal(1, intake.CurrentRevisionNumber);
            Assert.Single(intake.Revisions);
            Assert.Equal(firstSaveAtUtc, intake.LastEffectiveSavedAtUtc);
            Assert.Equal(expiryAfterFirstSave, intake.ExpiresAtUtc);
        }

        [Fact]
        public void SaveDraft_RejectsMissingDuplicateAndUnknownQuestionKeys()
        {
            var account = PatientPortalAccount.CreateUnlinked(
                Guid.NewGuid(),
                "new.patient",
                "versioned-hash");
            var intake = PatientIntake.CreateForNewPatient(
                account,
                branch: null,
                CreatedAtUtc);
            var empty = PatientIntakeDraftData.Empty();

            var missing = empty with
            {
                MedicalAnswers = empty.MedicalAnswers.Skip(1).ToArray()
            };
            Assert.Throws<ArgumentException>(() => intake.SaveDraft(
                missing,
                account.Id,
                CreatedAtUtc.AddMinutes(1),
                "missing"));

            var duplicate = empty with
            {
                MedicalAnswers = empty.MedicalAnswers
                    .Concat(new[] { empty.MedicalAnswers[0] })
                    .ToArray()
            };
            Assert.Throws<ArgumentException>(() => intake.SaveDraft(
                duplicate,
                account.Id,
                CreatedAtUtc.AddMinutes(1),
                "duplicate"));

            var unknownAnswers = empty.MedicalAnswers.ToArray();
            unknownAnswers[0] = unknownAnswers[0] with { QuestionKey = "unsupportedQuestion" };
            var unknown = empty with { MedicalAnswers = unknownAnswers };
            Assert.Throws<ArgumentException>(() => intake.SaveDraft(
                unknown,
                account.Id,
                CreatedAtUtc.AddMinutes(1),
                "unknown"));
        }

        [Fact]
        public void SaveDraft_RejectsWrongActorAndExpiredDraft()
        {
            var account = PatientPortalAccount.CreateUnlinked(
                Guid.NewGuid(),
                "new.patient",
                "versioned-hash");
            var intake = PatientIntake.CreateForNewPatient(
                account,
                branch: null,
                CreatedAtUtc,
                TimeSpan.FromMinutes(30));
            var draft = BuildDraft(firstName: "Ana");

            Assert.Throws<InvalidOperationException>(() => intake.SaveDraft(
                draft,
                Guid.NewGuid(),
                CreatedAtUtc.AddMinutes(1),
                "wrong-actor"));

            Assert.Throws<InvalidOperationException>(() => intake.SaveDraft(
                draft,
                account.Id,
                CreatedAtUtc.AddMinutes(30),
                "expired"));

            Assert.True(intake.ExpireIfDue(CreatedAtUtc.AddMinutes(30)));
            Assert.Equal(PatientIntakeStatus.Expired, intake.Status);
            Assert.False(intake.ExpireIfDue(CreatedAtUtc.AddMinutes(31)));
        }

        [Fact]
        public void SaveDraft_RejectsFutureBirthDateInvalidEmailAndIncompleteResponsibleParty()
        {
            var account = PatientPortalAccount.CreateUnlinked(
                Guid.NewGuid(),
                "new.patient",
                "versioned-hash");
            var intake = PatientIntake.CreateForNewPatient(
                account,
                branch: null,
                CreatedAtUtc);
            var empty = PatientIntakeDraftData.Empty();
            var occurredAtUtc = CreatedAtUtc.AddMinutes(1);

            Assert.Throws<ArgumentException>(() => intake.SaveDraft(
                empty with { DateOfBirth = new DateOnly(2027, 1, 1) },
                account.Id,
                occurredAtUtc,
                "future-date"));

            Assert.Throws<ArgumentException>(() => intake.SaveDraft(
                empty with { Email = "invalid email" },
                account.Id,
                occurredAtUtc,
                "invalid-email"));

            Assert.Throws<ArgumentException>(() => intake.SaveDraft(
                empty with { ResponsiblePartyPhone = "555-0100" },
                account.Id,
                occurredAtUtc,
                "responsible-party"));
        }

        [Fact]
        public void Submit_RequiresCompleteDemographicsAndEveryMedicalAnswer()
        {
            var account = PatientPortalAccount.CreateUnlinked(
                Guid.NewGuid(),
                "new.patient",
                "versioned-hash");
            var intake = PatientIntake.CreateForNewPatient(
                account,
                branch: null,
                CreatedAtUtc);

            Assert.False(intake.IsReadyForSubmission());
            Assert.Throws<InvalidOperationException>(() => intake.Submit(
                account.Id,
                CreatedAtUtc.AddMinutes(1),
                "incomplete-submit"));

            intake.SaveDraft(
                BuildDraft(
                    firstName: "Ana",
                    lastName: "Lopez",
                    dateOfBirth: new DateOnly(1991, 2, 14),
                    answerAllQuestions: true),
                account.Id,
                CreatedAtUtc.AddMinutes(2),
                "complete-draft");

            Assert.True(intake.IsReadyForSubmission());
        }

        [Fact]
        public void Submit_CreatesFinalRevisionAndMakesIntakeImmutableAndNonExpiring()
        {
            var account = PatientPortalAccount.CreateUnlinked(
                Guid.NewGuid(),
                "new.patient",
                "versioned-hash");
            var intake = PatientIntake.CreateForNewPatient(
                account,
                branch: null,
                CreatedAtUtc);
            intake.SaveDraft(
                BuildDraft(
                    firstName: "Ana",
                    lastName: "Lopez",
                    dateOfBirth: new DateOnly(1991, 2, 14),
                    answerAllQuestions: true),
                account.Id,
                CreatedAtUtc.AddMinutes(1),
                "complete-draft");
            var submittedAtUtc = CreatedAtUtc.AddMinutes(2);

            var revision = intake.Submit(
                account.Id,
                submittedAtUtc,
                "submit-1");

            Assert.Equal(PatientIntakeStatus.Submitted, intake.Status);
            Assert.Equal(submittedAtUtc, intake.SubmittedAtUtc);
            Assert.Equal(submittedAtUtc, intake.LastUpdatedAtUtc);
            Assert.Equal(2, intake.CurrentRevisionNumber);
            Assert.Equal(2, intake.Revisions.Count);
            Assert.Contains("status", revision.ChangedFieldsJson, StringComparison.Ordinal);
            Assert.Equal("submit-1", revision.CorrelationId);
            Assert.False(intake.IsExpiredAt(intake.ExpiresAtUtc.AddYears(1)));
            Assert.False(intake.ExpireIfDue(intake.ExpiresAtUtc.AddYears(1)));

            Assert.Throws<InvalidOperationException>(() => intake.SaveDraft(
                BuildDraft(
                    firstName: "Changed",
                    lastName: "Lopez",
                    dateOfBirth: new DateOnly(1991, 2, 14),
                    answerAllQuestions: true),
                account.Id,
                submittedAtUtc.AddMinutes(1),
                "post-submit-save"));
            Assert.Throws<InvalidOperationException>(() => intake.Submit(
                account.Id,
                submittedAtUtc.AddMinutes(1),
                "second-submit"));
        }

        private static PatientIntakeDraftData BuildDraft(
            string? firstName = null,
            string? lastName = null,
            DateOnly? dateOfBirth = null,
            string? reasonForVisit = null,
            ClinicalMedicalAnswerValue diabetesAnswer = ClinicalMedicalAnswerValue.Unknown,
            string? diabetesDetails = null,
            bool answerAllQuestions = false)
        {
            var empty = PatientIntakeDraftData.Empty();
            return empty with
            {
                FirstName = firstName,
                LastName = lastName,
                DateOfBirth = dateOfBirth,
                ReasonForVisit = reasonForVisit,
                MedicalAnswers = empty.MedicalAnswers
                    .Select(answer => answer.QuestionKey == "diabetes"
                        ? answer with
                        {
                            Answer = answerAllQuestions &&
                                     diabetesAnswer == ClinicalMedicalAnswerValue.Unknown
                                ? ClinicalMedicalAnswerValue.No
                                : diabetesAnswer,
                            Details = diabetesDetails
                        }
                        : answerAllQuestions
                            ? answer with { Answer = ClinicalMedicalAnswerValue.No }
                            : answer)
                    .ToArray()
            };
        }

        private static Patient CreatePatient(Guid tenantId)
        {
            return new Patient(
                tenantId,
                "Ana",
                "Lopez",
                new DateOnly(1991, 2, 14));
        }
    }
}
