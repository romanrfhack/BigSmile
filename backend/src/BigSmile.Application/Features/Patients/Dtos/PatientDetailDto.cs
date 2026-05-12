namespace BigSmile.Application.Features.Patients.Dtos
{
    public sealed record ResponsiblePartyDto(
        string Name,
        string? Relationship,
        string? Phone);

    public sealed record PatientSummaryDto(
        Guid Id,
        string FirstName,
        string LastName,
        string FullName,
        DateOnly DateOfBirth,
        string Sex,
        string? Occupation,
        string MaritalStatus,
        string? ReferredBy,
        string? PrimaryPhone,
        string? Email,
        bool IsActive,
        bool HasClinicalAlerts);

    public sealed record PatientDetailDto(
        Guid Id,
        string FirstName,
        string LastName,
        string FullName,
        DateOnly DateOfBirth,
        string Sex,
        string? Occupation,
        string MaritalStatus,
        string? ReferredBy,
        string? PrimaryPhone,
        string? Email,
        bool IsActive,
        bool HasClinicalAlerts,
        string? ClinicalAlertsSummary,
        ResponsiblePartyDto? ResponsibleParty,
        DateTime CreatedAt,
        DateTime? UpdatedAt);
}
