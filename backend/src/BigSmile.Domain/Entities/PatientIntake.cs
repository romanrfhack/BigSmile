using System.ComponentModel.DataAnnotations;
using BigSmile.SharedKernel;
using BigSmile.SharedKernel.Multitenancy;

namespace BigSmile.Domain.Entities
{
    public sealed class PatientIntake : Entity<Guid>, ITenantOwnedEntity
    {
        public const int NameMaxLength = 100;
        public const int DemographicMaxLength = 100;
        public const int PhoneMaxLength = 40;
        public const int EmailMaxLength = 256;
        public const int ReasonForVisitMaxLength = 500;

        public static readonly TimeSpan DefaultDraftLifetime = TimeSpan.FromDays(30);
        public static readonly TimeSpan MaximumDraftLifetime = TimeSpan.FromDays(365);

        private readonly List<PatientIntakeMedicalAnswer> _medicalAnswers = new();
        private readonly List<PatientIntakeRevision> _revisions = new();

        public Guid TenantId { get; private set; }
        public Tenant Tenant { get; private set; } = null!;

        public Guid PatientPortalAccountId { get; private set; }
        public PatientPortalAccount PatientPortalAccount { get; private set; } = null!;

        public Guid? PatientId { get; private set; }
        public Patient? Patient { get; private set; }

        public Guid? BranchId { get; private set; }
        public Branch? Branch { get; private set; }

        public PatientIntakeOrigin Origin { get; private set; }
        public PatientIntakeStatus Status { get; private set; } = PatientIntakeStatus.Draft;

        public string? FirstName { get; private set; }
        public string? LastName { get; private set; }
        public DateOnly? DateOfBirth { get; private set; }
        public PatientSex Sex { get; private set; } = PatientSex.Unspecified;
        public string? Occupation { get; private set; }
        public PatientMaritalStatus MaritalStatus { get; private set; } = PatientMaritalStatus.Unspecified;
        public string? ReferredBy { get; private set; }

        public string? PreferredPhone { get; private set; }
        public string? MobilePhone { get; private set; }
        public string? HomePhone { get; private set; }
        public string? WorkPhone { get; private set; }
        public string? Email { get; private set; }

        public string? ResponsiblePartyName { get; private set; }
        public string? ResponsiblePartyRelationship { get; private set; }
        public string? ResponsiblePartyPhone { get; private set; }

        public string? ReasonForVisit { get; private set; }

        public string? CanonicalPatientBaselineJson { get; private set; }
        public DateTime? CanonicalPatientBaselineCapturedAtUtc { get; private set; }

        public int CurrentRevisionNumber { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }
        public DateTime LastUpdatedAtUtc { get; private set; }
        public DateTime? LastEffectiveSavedAtUtc { get; private set; }
        public DateTime ExpiresAtUtc { get; private set; }
        public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

        public IReadOnlyCollection<PatientIntakeMedicalAnswer> MedicalAnswers => _medicalAnswers.AsReadOnly();
        public IReadOnlyCollection<PatientIntakeRevision> Revisions => _revisions.AsReadOnly();

        private PatientIntake()
        {
        }

        private PatientIntake(
            PatientPortalAccount account,
            PatientIntakeOrigin origin,
            Patient? patient,
            Branch? branch,
            PatientIntakeDraftData initialDraft,
            DateTime createdAtUtc,
            TimeSpan draftLifetime,
            string? canonicalPatientBaselineJson)
        {
            ArgumentNullException.ThrowIfNull(account);
            ArgumentNullException.ThrowIfNull(initialDraft);
            EnsureUtc(createdAtUtc, nameof(createdAtUtc));
            EnsureDraftLifetime(draftLifetime);
            EnsureDefinedEnum(origin, nameof(origin));

            if (!account.IsActive)
            {
                throw new InvalidOperationException(
                    "An inactive patient portal account cannot own an intake draft.");
            }

            EnsureBranchOwnership(branch, account.TenantId);

            Id = Guid.NewGuid();
            TenantId = account.TenantId;
            PatientPortalAccountId = account.Id;
            PatientPortalAccount = account;
            PatientId = patient?.Id;
            Patient = patient;
            BranchId = branch?.Id;
            Branch = branch;
            Origin = origin;
            Status = PatientIntakeStatus.Draft;
            CreatedAtUtc = createdAtUtc;
            LastUpdatedAtUtc = createdAtUtc;
            ExpiresAtUtc = createdAtUtc.Add(draftLifetime);

            var normalizedInitialDraft = NormalizeDraft(initialDraft, createdAtUtc);
            ApplyInitialDraft(normalizedInitialDraft, createdAtUtc);

            CanonicalPatientBaselineJson = canonicalPatientBaselineJson;
            CanonicalPatientBaselineCapturedAtUtc = canonicalPatientBaselineJson is null
                ? null
                : createdAtUtc;
        }

        public static PatientIntake CreateForExistingPatient(
            PatientPortalAccount account,
            Patient patient,
            Branch? branch,
            DateTime createdAtUtc,
            TimeSpan? draftLifetime = null)
        {
            ArgumentNullException.ThrowIfNull(account);
            ArgumentNullException.ThrowIfNull(patient);

            if (!patient.IsActive)
            {
                throw new InvalidOperationException(
                    "An inactive patient cannot start a patient intake draft.");
            }

            if (account.TenantId != patient.TenantId || account.PatientId != patient.Id)
            {
                throw new InvalidOperationException(
                    "An existing-patient intake requires a portal account linked to the same patient and tenant.");
            }

            var initialDraft = BuildExistingPatientInitialDraft(patient);
            var normalizedInitialDraft = NormalizeDraft(initialDraft, createdAtUtc);
            var baselineJson = PatientIntakeSnapshotSerializer.SerializeSnapshot(normalizedInitialDraft);

            return new PatientIntake(
                account,
                PatientIntakeOrigin.ExistingPatientPortal,
                patient,
                branch,
                normalizedInitialDraft,
                createdAtUtc,
                draftLifetime ?? DefaultDraftLifetime,
                baselineJson);
        }

        public static PatientIntake CreateForNewPatient(
            PatientPortalAccount account,
            Branch? branch,
            DateTime createdAtUtc,
            TimeSpan? draftLifetime = null)
        {
            ArgumentNullException.ThrowIfNull(account);

            if (account.PatientId.HasValue)
            {
                throw new InvalidOperationException(
                    "A waiting-room intake requires an unlinked patient portal account.");
            }

            return new PatientIntake(
                account,
                PatientIntakeOrigin.NewPatientWaitingRoom,
                patient: null,
                branch,
                PatientIntakeDraftData.Empty(),
                createdAtUtc,
                draftLifetime ?? DefaultDraftLifetime,
                canonicalPatientBaselineJson: null);
        }

        public PatientIntakeRevision? SaveDraft(
            PatientIntakeDraftData draft,
            Guid actorPatientPortalAccountId,
            DateTime occurredAtUtc,
            string correlationId,
            TimeSpan? draftLifetime = null)
        {
            ArgumentNullException.ThrowIfNull(draft);
            EnsureUtc(occurredAtUtc, nameof(occurredAtUtc));

            if (actorPatientPortalAccountId == Guid.Empty ||
                actorPatientPortalAccountId != PatientPortalAccountId)
            {
                throw new InvalidOperationException(
                    "Only the owning patient portal account can save this intake draft.");
            }

            if (Status != PatientIntakeStatus.Draft || occurredAtUtc >= ExpiresAtUtc)
            {
                throw new InvalidOperationException(
                    "An expired patient intake draft cannot be changed.");
            }

            var effectiveLifetime = draftLifetime ?? DefaultDraftLifetime;
            EnsureDraftLifetime(effectiveLifetime);

            var normalizedDraft = NormalizeDraft(draft, occurredAtUtc);
            var changedFields = GetChangedFields(normalizedDraft);
            if (changedFields.Count == 0)
            {
                return null;
            }

            ApplyDraft(normalizedDraft, occurredAtUtc);
            CurrentRevisionNumber = checked(CurrentRevisionNumber + 1);
            LastEffectiveSavedAtUtc = occurredAtUtc;
            LastUpdatedAtUtc = occurredAtUtc;
            ExpiresAtUtc = occurredAtUtc.Add(effectiveLifetime);

            var revision = new PatientIntakeRevision(
                this,
                CurrentRevisionNumber,
                actorPatientPortalAccountId,
                occurredAtUtc,
                PatientIntakeSnapshotSerializer.SerializeChangedFields(changedFields),
                PatientIntakeSnapshotSerializer.SerializeSnapshot(GetDraftData()),
                correlationId);

            _revisions.Add(revision);
            return revision;
        }

        public bool ExpireIfDue(DateTime occurredAtUtc)
        {
            EnsureUtc(occurredAtUtc, nameof(occurredAtUtc));

            if (Status == PatientIntakeStatus.Expired || occurredAtUtc < ExpiresAtUtc)
            {
                return false;
            }

            Status = PatientIntakeStatus.Expired;
            LastUpdatedAtUtc = occurredAtUtc;
            return true;
        }

        public bool IsExpiredAt(DateTime occurredAtUtc)
        {
            EnsureUtc(occurredAtUtc, nameof(occurredAtUtc));
            return Status == PatientIntakeStatus.Expired || occurredAtUtc >= ExpiresAtUtc;
        }

        public PatientIntakeDraftData GetDraftData()
        {
            var answersByKey = _medicalAnswers.ToDictionary(
                answer => answer.QuestionKey,
                StringComparer.Ordinal);

            var orderedAnswers = ClinicalMedicalQuestionnaireCatalog.AllowedQuestionKeys
                .Select(questionKey =>
                {
                    if (!answersByKey.TryGetValue(questionKey, out var answer))
                    {
                        throw new InvalidOperationException(
                            $"Patient intake answer '{questionKey}' is missing from the aggregate.");
                    }

                    return new PatientIntakeMedicalAnswerData(
                        answer.QuestionKey,
                        answer.Answer,
                        answer.Details);
                })
                .ToArray();

            return new PatientIntakeDraftData(
                FirstName,
                LastName,
                DateOfBirth,
                Sex,
                Occupation,
                MaritalStatus,
                ReferredBy,
                PreferredPhone,
                MobilePhone,
                HomePhone,
                WorkPhone,
                Email,
                ResponsiblePartyName,
                ResponsiblePartyRelationship,
                ResponsiblePartyPhone,
                ReasonForVisit,
                orderedAnswers);
        }

        private static PatientIntakeDraftData BuildExistingPatientInitialDraft(Patient patient)
        {
            var empty = PatientIntakeDraftData.Empty();
            return empty with
            {
                FirstName = patient.FirstName,
                LastName = patient.LastName,
                DateOfBirth = patient.DateOfBirth,
                Sex = patient.Sex,
                Occupation = patient.Occupation,
                MaritalStatus = patient.MaritalStatus,
                ReferredBy = patient.ReferredBy,
                PreferredPhone = patient.PrimaryPhone,
                Email = patient.Email,
                ResponsiblePartyName = patient.ResponsiblePartyName,
                ResponsiblePartyRelationship = patient.ResponsiblePartyRelationship,
                ResponsiblePartyPhone = patient.ResponsiblePartyPhone
            };
        }

        private void ApplyInitialDraft(
            PatientIntakeDraftData normalizedDraft,
            DateTime occurredAtUtc)
        {
            ApplyScalarFields(normalizedDraft);

            foreach (var answer in normalizedDraft.MedicalAnswers)
            {
                _medicalAnswers.Add(new PatientIntakeMedicalAnswer(
                    this,
                    answer.QuestionKey,
                    answer.Answer,
                    answer.Details,
                    occurredAtUtc));
            }
        }

        private void ApplyDraft(
            PatientIntakeDraftData normalizedDraft,
            DateTime occurredAtUtc)
        {
            ApplyScalarFields(normalizedDraft);

            var answersByKey = _medicalAnswers.ToDictionary(
                answer => answer.QuestionKey,
                StringComparer.Ordinal);

            foreach (var answer in normalizedDraft.MedicalAnswers)
            {
                if (!answersByKey.TryGetValue(answer.QuestionKey, out var currentAnswer))
                {
                    throw new InvalidOperationException(
                        $"Patient intake answer '{answer.QuestionKey}' is missing from the aggregate.");
                }

                currentAnswer.Update(answer.Answer, answer.Details, occurredAtUtc);
            }
        }

        private void ApplyScalarFields(PatientIntakeDraftData draft)
        {
            FirstName = draft.FirstName;
            LastName = draft.LastName;
            DateOfBirth = draft.DateOfBirth;
            Sex = draft.Sex;
            Occupation = draft.Occupation;
            MaritalStatus = draft.MaritalStatus;
            ReferredBy = draft.ReferredBy;
            PreferredPhone = draft.PreferredPhone;
            MobilePhone = draft.MobilePhone;
            HomePhone = draft.HomePhone;
            WorkPhone = draft.WorkPhone;
            Email = draft.Email;
            ResponsiblePartyName = draft.ResponsiblePartyName;
            ResponsiblePartyRelationship = draft.ResponsiblePartyRelationship;
            ResponsiblePartyPhone = draft.ResponsiblePartyPhone;
            ReasonForVisit = draft.ReasonForVisit;
        }

        private IReadOnlyList<string> GetChangedFields(PatientIntakeDraftData draft)
        {
            var changedFields = new List<string>();

            AddChangedField(changedFields, "firstName", FirstName, draft.FirstName);
            AddChangedField(changedFields, "lastName", LastName, draft.LastName);
            AddChangedField(changedFields, "dateOfBirth", DateOfBirth, draft.DateOfBirth);
            AddChangedField(changedFields, "sex", Sex, draft.Sex);
            AddChangedField(changedFields, "occupation", Occupation, draft.Occupation);
            AddChangedField(changedFields, "maritalStatus", MaritalStatus, draft.MaritalStatus);
            AddChangedField(changedFields, "referredBy", ReferredBy, draft.ReferredBy);
            AddChangedField(changedFields, "preferredPhone", PreferredPhone, draft.PreferredPhone);
            AddChangedField(changedFields, "mobilePhone", MobilePhone, draft.MobilePhone);
            AddChangedField(changedFields, "homePhone", HomePhone, draft.HomePhone);
            AddChangedField(changedFields, "workPhone", WorkPhone, draft.WorkPhone);
            AddChangedField(changedFields, "email", Email, draft.Email);
            AddChangedField(
                changedFields,
                "responsiblePartyName",
                ResponsiblePartyName,
                draft.ResponsiblePartyName);
            AddChangedField(
                changedFields,
                "responsiblePartyRelationship",
                ResponsiblePartyRelationship,
                draft.ResponsiblePartyRelationship);
            AddChangedField(
                changedFields,
                "responsiblePartyPhone",
                ResponsiblePartyPhone,
                draft.ResponsiblePartyPhone);
            AddChangedField(changedFields, "reasonForVisit", ReasonForVisit, draft.ReasonForVisit);

            var currentAnswers = _medicalAnswers.ToDictionary(
                answer => answer.QuestionKey,
                StringComparer.Ordinal);

            foreach (var proposedAnswer in draft.MedicalAnswers)
            {
                if (!currentAnswers.TryGetValue(proposedAnswer.QuestionKey, out var currentAnswer) ||
                    currentAnswer.Answer != proposedAnswer.Answer ||
                    !string.Equals(
                        currentAnswer.Details,
                        proposedAnswer.Details,
                        StringComparison.Ordinal))
                {
                    changedFields.Add($"medicalAnswers.{proposedAnswer.QuestionKey}");
                }
            }

            return changedFields
                .Distinct(StringComparer.Ordinal)
                .OrderBy(field => field, StringComparer.Ordinal)
                .ToArray();
        }

        private static void AddChangedField<T>(
            ICollection<string> changedFields,
            string fieldName,
            T currentValue,
            T proposedValue)
        {
            if (!EqualityComparer<T>.Default.Equals(currentValue, proposedValue))
            {
                changedFields.Add(fieldName);
            }
        }

        private static PatientIntakeDraftData NormalizeDraft(
            PatientIntakeDraftData draft,
            DateTime occurredAtUtc)
        {
            ArgumentNullException.ThrowIfNull(draft);
            EnsureUtc(occurredAtUtc, nameof(occurredAtUtc));

            var firstName = NormalizeOptional(draft.FirstName, nameof(draft.FirstName), NameMaxLength);
            var lastName = NormalizeOptional(draft.LastName, nameof(draft.LastName), NameMaxLength);
            var dateOfBirth = NormalizeDateOfBirth(draft.DateOfBirth, occurredAtUtc);
            var sex = EnsureDefinedEnum(draft.Sex, nameof(draft.Sex));
            var occupation = NormalizeOptional(
                draft.Occupation,
                nameof(draft.Occupation),
                DemographicMaxLength);
            var maritalStatus = EnsureDefinedEnum(
                draft.MaritalStatus,
                nameof(draft.MaritalStatus));
            var referredBy = NormalizeOptional(
                draft.ReferredBy,
                nameof(draft.ReferredBy),
                DemographicMaxLength);
            var preferredPhone = NormalizeOptional(
                draft.PreferredPhone,
                nameof(draft.PreferredPhone),
                PhoneMaxLength);
            var mobilePhone = NormalizeOptional(
                draft.MobilePhone,
                nameof(draft.MobilePhone),
                PhoneMaxLength);
            var homePhone = NormalizeOptional(
                draft.HomePhone,
                nameof(draft.HomePhone),
                PhoneMaxLength);
            var workPhone = NormalizeOptional(
                draft.WorkPhone,
                nameof(draft.WorkPhone),
                PhoneMaxLength);
            var email = NormalizeOptional(draft.Email, nameof(draft.Email), EmailMaxLength);
            if (email is not null && !new EmailAddressAttribute().IsValid(email))
            {
                throw new ArgumentException(
                    "Patient intake email must be a valid email address.",
                    nameof(draft.Email));
            }

            var responsiblePartyName = NormalizeOptional(
                draft.ResponsiblePartyName,
                nameof(draft.ResponsiblePartyName),
                NameMaxLength);
            var responsiblePartyRelationship = NormalizeOptional(
                draft.ResponsiblePartyRelationship,
                nameof(draft.ResponsiblePartyRelationship),
                DemographicMaxLength);
            var responsiblePartyPhone = NormalizeOptional(
                draft.ResponsiblePartyPhone,
                nameof(draft.ResponsiblePartyPhone),
                PhoneMaxLength);

            if ((responsiblePartyRelationship is not null || responsiblePartyPhone is not null) &&
                responsiblePartyName is null)
            {
                throw new ArgumentException(
                    "Responsible party name is required when relationship or phone is provided.",
                    nameof(draft.ResponsiblePartyName));
            }

            var reasonForVisit = NormalizeOptional(
                draft.ReasonForVisit,
                nameof(draft.ReasonForVisit),
                ReasonForVisitMaxLength);

            if (draft.MedicalAnswers is null)
            {
                throw new ArgumentException(
                    "Patient intake medical answers are required.",
                    nameof(draft.MedicalAnswers));
            }

            var answersByKey = new Dictionary<string, PatientIntakeMedicalAnswerData>(
                StringComparer.Ordinal);
            foreach (var proposedAnswer in draft.MedicalAnswers)
            {
                if (proposedAnswer is null)
                {
                    throw new ArgumentException(
                        "Patient intake medical answers cannot contain null entries.",
                        nameof(draft.MedicalAnswers));
                }

                var questionKey = ClinicalMedicalQuestionnaireCatalog.NormalizeQuestionKey(
                    proposedAnswer.QuestionKey);
                var answer = EnsureDefinedEnum(
                    proposedAnswer.Answer,
                    nameof(proposedAnswer.Answer));
                var details = NormalizeOptional(
                    proposedAnswer.Details,
                    nameof(proposedAnswer.Details),
                    ClinicalMedicalAnswer.DetailsMaxLength);

                if (!answersByKey.TryAdd(
                        questionKey,
                        new PatientIntakeMedicalAnswerData(questionKey, answer, details)))
                {
                    throw new ArgumentException(
                        $"Patient intake medical answer '{questionKey}' is duplicated.",
                        nameof(draft.MedicalAnswers));
                }
            }

            if (answersByKey.Count != ClinicalMedicalQuestionnaireCatalog.AllowedQuestionKeys.Count)
            {
                throw new ArgumentException(
                    "Patient intake medical answers must include the complete fixed questionnaire catalog.",
                    nameof(draft.MedicalAnswers));
            }

            var orderedAnswers = ClinicalMedicalQuestionnaireCatalog.AllowedQuestionKeys
                .Select(questionKey =>
                {
                    if (!answersByKey.TryGetValue(questionKey, out var answer))
                    {
                        throw new ArgumentException(
                            $"Patient intake medical answer '{questionKey}' is required.",
                            nameof(draft.MedicalAnswers));
                    }

                    return answer;
                })
                .ToArray();

            return new PatientIntakeDraftData(
                firstName,
                lastName,
                dateOfBirth,
                sex,
                occupation,
                maritalStatus,
                referredBy,
                preferredPhone,
                mobilePhone,
                homePhone,
                workPhone,
                email,
                responsiblePartyName,
                responsiblePartyRelationship,
                responsiblePartyPhone,
                reasonForVisit,
                orderedAnswers);
        }

        private static DateOnly? NormalizeDateOfBirth(
            DateOnly? value,
            DateTime occurredAtUtc)
        {
            if (!value.HasValue)
            {
                return null;
            }

            var currentDate = DateOnly.FromDateTime(occurredAtUtc);
            if (value.Value > currentDate)
            {
                throw new ArgumentException(
                    "Patient intake date of birth cannot be in the future.",
                    nameof(value));
            }

            return value;
        }

        private static string? NormalizeOptional(
            string? value,
            string paramName,
            int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var normalized = value.Trim();
            if (normalized.Length > maxLength)
            {
                throw new ArgumentException(
                    $"{paramName} exceeds the allowed length of {maxLength}.",
                    paramName);
            }

            return normalized;
        }

        private static TEnum EnsureDefinedEnum<TEnum>(TEnum value, string paramName)
            where TEnum : struct, Enum
        {
            if (!Enum.IsDefined(typeof(TEnum), value))
            {
                throw new ArgumentException(
                    $"{paramName} is not supported.",
                    paramName);
            }

            return value;
        }

        private static void EnsureBranchOwnership(Branch? branch, Guid tenantId)
        {
            if (branch is null)
            {
                return;
            }

            if (branch.TenantId != tenantId)
            {
                throw new InvalidOperationException(
                    "Patient intake branch must belong to the same tenant as the portal account.");
            }

            if (!branch.IsActive)
            {
                throw new InvalidOperationException(
                    "An inactive branch cannot be used as patient intake context.");
            }
        }

        private static void EnsureDraftLifetime(TimeSpan draftLifetime)
        {
            if (draftLifetime <= TimeSpan.Zero || draftLifetime > MaximumDraftLifetime)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(draftLifetime),
                    $"Patient intake draft lifetime must be greater than zero and no more than {MaximumDraftLifetime.TotalDays:0} days.");
            }
        }

        private static void EnsureUtc(DateTime value, string paramName)
        {
            if (value.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException(
                    "Patient intake timestamps must be UTC.",
                    paramName);
            }
        }
    }
}
