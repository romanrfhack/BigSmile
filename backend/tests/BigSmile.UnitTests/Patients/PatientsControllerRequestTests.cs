using System.ComponentModel.DataAnnotations;
using BigSmile.Api.Controllers;
using BigSmile.Domain.Entities;

namespace BigSmile.UnitTests.Patients
{
    public class PatientsControllerRequestTests
    {
        [Fact]
        public void SavePatientRequest_ToCommand_MapsControlledDemographics()
        {
            var request = new PatientsController.SavePatientRequest
            {
                FirstName = "Ana",
                LastName = "Lopez",
                DateOfBirth = new DateOnly(1991, 2, 14),
                Sex = "Female",
                Occupation = "Dentist",
                MaritalStatus = "Single",
                ReferredBy = "Existing patient"
            };

            var command = request.ToCommand();

            Assert.Equal(PatientSex.Female, command.Sex);
            Assert.Equal("Dentist", command.Occupation);
            Assert.Equal(PatientMaritalStatus.Single, command.MaritalStatus);
            Assert.Equal("Existing patient", command.ReferredBy);
        }

        [Fact]
        public void SavePatientRequest_Validate_RejectsNumericEnumValues()
        {
            var request = new PatientsController.SavePatientRequest
            {
                FirstName = "Ana",
                LastName = "Lopez",
                DateOfBirth = new DateOnly(1991, 2, 14),
                Sex = "1",
                MaritalStatus = "2"
            };

            var results = request.Validate(new ValidationContext(request)).ToList();

            Assert.Contains(results, result => result.MemberNames.Contains(nameof(PatientsController.SavePatientRequest.Sex)));
            Assert.Contains(results, result => result.MemberNames.Contains(nameof(PatientsController.SavePatientRequest.MaritalStatus)));
        }
    }
}
