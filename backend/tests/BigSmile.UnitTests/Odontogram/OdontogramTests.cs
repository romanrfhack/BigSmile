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
            Assert.Equal(160, odontogram.Surfaces.Count);
            Assert.Equal(
                160,
                odontogram.Surfaces
                    .Select(surface => $"{surface.ToothCode}:{surface.SurfaceCode}")
                    .Distinct(StringComparer.Ordinal)
                    .Count());
            Assert.All(odontogram.Surfaces, surface => Assert.Equal(OdontogramSurfaceStatus.Unknown, surface.Status));
            Assert.All(odontogram.Surfaces, surface => Assert.Equal(actorUserId, surface.UpdatedByUserId));
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
        public void UpdateSurfaceStatus_RejectsInvalidToothCode()
        {
            var odontogram = CreateOdontogram();

            var exception = Assert.Throws<ArgumentException>(() => odontogram.UpdateSurfaceStatus("55", "O", OdontogramSurfaceStatus.Caries, Guid.NewGuid()));

            Assert.Contains("FDI permanent adult numbering", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void UpdateSurfaceStatus_RejectsInvalidSurfaceCode()
        {
            var odontogram = CreateOdontogram();

            var exception = Assert.Throws<ArgumentException>(() => odontogram.UpdateSurfaceStatus("11", "X", OdontogramSurfaceStatus.Caries, Guid.NewGuid()));

            Assert.Contains("Surface code must be one of", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void UpdateSurfaceStatus_ChangesStatusAndMetadata()
        {
            var odontogram = CreateOdontogram();
            var actorUserId = Guid.NewGuid();
            var originalUpdatedAtUtc = odontogram.LastUpdatedAtUtc;

            var changed = odontogram.UpdateSurfaceStatus("11", "O", OdontogramSurfaceStatus.Caries, actorUserId);

            Assert.True(changed);
            var surface = odontogram.Surfaces.Single(entry => entry.ToothCode == "11" && entry.SurfaceCode == "O");
            Assert.Equal(OdontogramSurfaceStatus.Caries, surface.Status);
            Assert.Equal(actorUserId, surface.UpdatedByUserId);
            Assert.Equal(actorUserId, odontogram.LastUpdatedByUserId);
            Assert.True(odontogram.LastUpdatedAtUtc >= originalUpdatedAtUtc);
        }

        [Fact]
        public void AddSurfaceFinding_RejectsInvalidToothCode()
        {
            var odontogram = CreateOdontogram();

            var exception = Assert.Throws<ArgumentException>(() =>
                odontogram.AddSurfaceFinding("55", "O", OdontogramSurfaceFindingType.Caries, Guid.NewGuid()));

            Assert.Contains("FDI permanent adult numbering", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void AddSurfaceFinding_RejectsInvalidSurfaceCode()
        {
            var odontogram = CreateOdontogram();

            var exception = Assert.Throws<ArgumentException>(() =>
                odontogram.AddSurfaceFinding("11", "X", OdontogramSurfaceFindingType.Caries, Guid.NewGuid()));

            Assert.Contains("Surface code must be one of", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void AddSurfaceFinding_CreatesFindingWithMetadata()
        {
            var odontogram = CreateOdontogram();
            var actorUserId = Guid.NewGuid();
            var originalUpdatedAtUtc = odontogram.LastUpdatedAtUtc;

            var finding = odontogram.AddSurfaceFinding("11", "O", OdontogramSurfaceFindingType.Caries, actorUserId);

            Assert.Equal("11", finding.ToothCode);
            Assert.Equal("O", finding.SurfaceCode);
            Assert.Equal(OdontogramSurfaceFindingType.Caries, finding.FindingType);
            Assert.Equal(actorUserId, finding.CreatedByUserId);
            Assert.Contains(odontogram.SurfaceFindings, entry => entry.Id == finding.Id);
            Assert.Equal(actorUserId, odontogram.LastUpdatedByUserId);
            Assert.True(odontogram.LastUpdatedAtUtc >= originalUpdatedAtUtc);
        }

        [Fact]
        public void AddSurfaceFinding_RejectsExactDuplicateOnSameSurface()
        {
            var odontogram = CreateOdontogram();
            odontogram.AddSurfaceFinding("11", "O", OdontogramSurfaceFindingType.Caries, Guid.NewGuid());

            var exception = Assert.Throws<InvalidOperationException>(() =>
                odontogram.AddSurfaceFinding("11", "O", OdontogramSurfaceFindingType.Caries, Guid.NewGuid()));

            Assert.Contains("already exists", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void RemoveSurfaceFinding_RemovesOnlyTheRequestedFinding()
        {
            var odontogram = CreateOdontogram();
            var keptFinding = odontogram.AddSurfaceFinding("11", "O", OdontogramSurfaceFindingType.Caries, Guid.NewGuid());
            var removedFinding = odontogram.AddSurfaceFinding("11", "O", OdontogramSurfaceFindingType.Sealant, Guid.NewGuid());
            var actorUserId = Guid.NewGuid();

            odontogram.RemoveSurfaceFinding("11", "O", removedFinding.Id, actorUserId);

            Assert.DoesNotContain(odontogram.SurfaceFindings, entry => entry.Id == removedFinding.Id);
            Assert.Contains(odontogram.SurfaceFindings, entry => entry.Id == keptFinding.Id);
            Assert.Equal(actorUserId, odontogram.LastUpdatedByUserId);
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
        public void UpdateSurfaceStatus_DoesNotTouchMetadataForNoOpUpdate()
        {
            var odontogram = CreateOdontogram();
            var originalSurface = odontogram.Surfaces.Single(entry => entry.ToothCode == "21" && entry.SurfaceCode == "M");
            var originalSurfaceUpdatedAtUtc = originalSurface.UpdatedAtUtc;
            var originalSurfaceUpdatedByUserId = originalSurface.UpdatedByUserId;
            var originalOdontogramUpdatedAtUtc = odontogram.LastUpdatedAtUtc;
            var originalOdontogramUpdatedByUserId = odontogram.LastUpdatedByUserId;

            var changed = odontogram.UpdateSurfaceStatus("21", "M", OdontogramSurfaceStatus.Unknown, Guid.NewGuid());

            Assert.False(changed);
            Assert.Equal(originalSurfaceUpdatedAtUtc, originalSurface.UpdatedAtUtc);
            Assert.Equal(originalSurfaceUpdatedByUserId, originalSurface.UpdatedByUserId);
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

        [Fact]
        public void SurfaceMutations_DoNotChangeTenantOrPatientOwnership()
        {
            var odontogram = CreateOdontogram();
            var originalTenantId = odontogram.TenantId;
            var originalPatientId = odontogram.PatientId;

            odontogram.UpdateSurfaceStatus("48", "L", OdontogramSurfaceStatus.Restored, Guid.NewGuid());

            Assert.Equal(originalTenantId, odontogram.TenantId);
            Assert.Equal(originalPatientId, odontogram.PatientId);
        }

        [Fact]
        public void SurfaceFindingMutations_DoNotChangeTenantOrPatientOwnership()
        {
            var odontogram = CreateOdontogram();
            var originalTenantId = odontogram.TenantId;
            var originalPatientId = odontogram.PatientId;
            var finding = odontogram.AddSurfaceFinding("48", "L", OdontogramSurfaceFindingType.Restoration, Guid.NewGuid());

            odontogram.RemoveSurfaceFinding("48", "L", finding.Id, Guid.NewGuid());

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
