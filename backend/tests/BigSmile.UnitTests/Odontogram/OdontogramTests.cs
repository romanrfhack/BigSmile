using BigSmile.Domain.Entities;

namespace BigSmile.UnitTests.Odontogram
{
    public class OdontogramTests
    {
        [Fact]
        public void CreateOdontogram_InitializesAdultPermanentTeethWithUnknownStatus()
        {
            var actorUserId = Guid.NewGuid();

            var odontogram = new Domain.Entities.Odontogram(
                Guid.NewGuid(),
                Guid.NewGuid(),
                actorUserId);

            Assert.Equal(32, odontogram.Teeth.Count);
            Assert.Equal(32, odontogram.Teeth.Select(tooth => tooth.ToothCode).Distinct(StringComparer.Ordinal).Count());
            Assert.All(odontogram.Teeth, tooth => Assert.Equal(OdontogramToothStatus.Unknown, tooth.Status));
            Assert.All(odontogram.Teeth, tooth => Assert.Equal(actorUserId, tooth.UpdatedByUserId));
        }

        [Fact]
        public void UpdateToothStatus_RejectsInvalidToothCode()
        {
            var odontogram = CreateOdontogram();

            var exception = Assert.Throws<ArgumentException>(() => odontogram.UpdateToothStatus("55", OdontogramToothStatus.Caries, Guid.NewGuid()));

            Assert.Contains("FDI permanent adult numbering", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void UpdateToothStatus_ChangesStatusAndMetadata()
        {
            var odontogram = CreateOdontogram();
            var actorUserId = Guid.NewGuid();
            var originalUpdatedAtUtc = odontogram.LastUpdatedAtUtc;

            var changed = odontogram.UpdateToothStatus("11", OdontogramToothStatus.Caries, actorUserId);

            Assert.True(changed);
            var tooth = odontogram.Teeth.Single(entry => entry.ToothCode == "11");
            Assert.Equal(OdontogramToothStatus.Caries, tooth.Status);
            Assert.Equal(actorUserId, tooth.UpdatedByUserId);
            Assert.Equal(actorUserId, odontogram.LastUpdatedByUserId);
            Assert.True(odontogram.LastUpdatedAtUtc >= originalUpdatedAtUtc);
        }

        [Fact]
        public void UpdateToothStatus_DoesNotTouchMetadataForNoOpUpdate()
        {
            var odontogram = CreateOdontogram();
            var originalTooth = odontogram.Teeth.Single(entry => entry.ToothCode == "21");
            var originalToothUpdatedAtUtc = originalTooth.UpdatedAtUtc;
            var originalToothUpdatedByUserId = originalTooth.UpdatedByUserId;
            var originalOdontogramUpdatedAtUtc = odontogram.LastUpdatedAtUtc;
            var originalOdontogramUpdatedByUserId = odontogram.LastUpdatedByUserId;

            var changed = odontogram.UpdateToothStatus("21", OdontogramToothStatus.Unknown, Guid.NewGuid());

            Assert.False(changed);
            Assert.Equal(originalToothUpdatedAtUtc, originalTooth.UpdatedAtUtc);
            Assert.Equal(originalToothUpdatedByUserId, originalTooth.UpdatedByUserId);
            Assert.Equal(originalOdontogramUpdatedAtUtc, odontogram.LastUpdatedAtUtc);
            Assert.Equal(originalOdontogramUpdatedByUserId, odontogram.LastUpdatedByUserId);
        }

        [Fact]
        public void ToothMutations_DoNotChangeTenantOrPatientOwnership()
        {
            var odontogram = CreateOdontogram();
            var originalTenantId = odontogram.TenantId;
            var originalPatientId = odontogram.PatientId;

            odontogram.UpdateToothStatus("48", OdontogramToothStatus.Missing, Guid.NewGuid());

            Assert.Equal(originalTenantId, odontogram.TenantId);
            Assert.Equal(originalPatientId, odontogram.PatientId);
        }

        private static Domain.Entities.Odontogram CreateOdontogram()
        {
            return new Domain.Entities.Odontogram(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid());
        }
    }
}
