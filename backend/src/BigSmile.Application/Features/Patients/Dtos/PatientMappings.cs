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
                patient.PrimaryPhone,
                patient.Email,
                patient.IsActive);
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
                patient.PrimaryPhone,
                patient.Email,
                patient.IsActive,
                responsibleParty,
                patient.CreatedAt,
                patient.UpdatedAt);
        }
    }
}
