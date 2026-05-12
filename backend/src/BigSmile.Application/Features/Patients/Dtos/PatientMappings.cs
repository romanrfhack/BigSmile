using BigSmile.Domain.Entities;

namespace BigSmile.Application.Features.Patients.Dtos
{
    internal static class PatientMappings
    {
        public static PatientSummaryDto ToSummaryDto(this Patient patient)
        {
            return new PatientSummaryDto(
                patient.Id,
                patient.FirstName,
                patient.LastName,
                patient.FullName,
                patient.DateOfBirth,
                patient.Sex.ToString(),
                patient.Occupation,
                patient.MaritalStatus.ToString(),
                patient.ReferredBy,
                patient.PrimaryPhone,
                patient.Email,
                patient.IsActive,
                patient.HasClinicalAlerts);
        }

        public static PatientDetailDto ToDetailDto(this Patient patient)
        {
            var responsibleParty = string.IsNullOrWhiteSpace(patient.ResponsiblePartyName)
                ? null
                : new ResponsiblePartyDto(
                    patient.ResponsiblePartyName,
                    patient.ResponsiblePartyRelationship,
                    patient.ResponsiblePartyPhone);

            return new PatientDetailDto(
                patient.Id,
                patient.FirstName,
                patient.LastName,
                patient.FullName,
                patient.DateOfBirth,
                patient.Sex.ToString(),
                patient.Occupation,
                patient.MaritalStatus.ToString(),
                patient.ReferredBy,
                patient.PrimaryPhone,
                patient.Email,
                patient.IsActive,
                patient.HasClinicalAlerts,
                patient.ClinicalAlertsSummary,
                responsibleParty,
                patient.CreatedAt,
                patient.UpdatedAt);
        }
    }
}
