using BigSmile.Domain.Entities;

namespace BigSmile.UnitTests.TreatmentPlans
{
    public class TreatmentPlanTests
    {
        [Fact]
        public void CreateTreatmentPlan_StartsAsDraft()
        {
            var treatmentPlan = CreateTreatmentPlan();

            Assert.Equal(TreatmentPlanStatus.Draft, treatmentPlan.Status);
            Assert.Empty(treatmentPlan.Items);
        }

        [Fact]
        public void AddItem_RejectsEmptyTitle()
        {
            var treatmentPlan = CreateTreatmentPlan();

            var exception = Assert.Throws<ArgumentException>(() =>
                treatmentPlan.AddItem("   ", null, 1, null, null, null, Guid.NewGuid()));

            Assert.Contains("title is required", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void AddItem_RejectsInvalidQuantity()
        {
            var treatmentPlan = CreateTreatmentPlan();

            var exception = Assert.Throws<ArgumentException>(() =>
                treatmentPlan.AddItem("Composite restoration", null, 0, null, null, null, Guid.NewGuid()));

            Assert.Contains("greater than zero", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void AddItem_RejectsSurfaceCodeWithoutToothCode()
        {
            var treatmentPlan = CreateTreatmentPlan();

            var exception = Assert.Throws<ArgumentException>(() =>
                treatmentPlan.AddItem("Composite restoration", null, 1, null, null, "O", Guid.NewGuid()));

            Assert.Contains("SurfaceCode requires ToothCode", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void AddItem_RejectsInvalidToothCode()
        {
            var treatmentPlan = CreateTreatmentPlan();

            var exception = Assert.Throws<ArgumentException>(() =>
                treatmentPlan.AddItem("Composite restoration", null, 1, null, "55", null, Guid.NewGuid()));

            Assert.Contains("FDI permanent adult numbering", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void AddItem_RejectsInvalidSurfaceCode()
        {
            var treatmentPlan = CreateTreatmentPlan();

            var exception = Assert.Throws<ArgumentException>(() =>
                treatmentPlan.AddItem("Composite restoration", null, 1, null, "11", "X", Guid.NewGuid()));

            Assert.Contains("Surface code must be one of", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void AddItem_AddsMetadataAndOptionalDentalLocation()
        {
            var treatmentPlan = CreateTreatmentPlan();
            var actorUserId = Guid.NewGuid();
            var originalUpdatedAtUtc = treatmentPlan.LastUpdatedAtUtc;

            var item = treatmentPlan.AddItem(
                "Composite restoration",
                "Restorative",
                2,
                "Two-surface restoration.",
                "11",
                "O",
                actorUserId);

            Assert.Equal("Composite restoration", item.Title);
            Assert.Equal("Restorative", item.Category);
            Assert.Equal(2, item.Quantity);
            Assert.Equal("Two-surface restoration.", item.Notes);
            Assert.Equal("11", item.ToothCode);
            Assert.Equal("O", item.SurfaceCode);
            Assert.Equal(actorUserId, item.CreatedByUserId);
            Assert.Contains(treatmentPlan.Items, entry => entry.Id == item.Id);
            Assert.Equal(actorUserId, treatmentPlan.LastUpdatedByUserId);
            Assert.True(treatmentPlan.LastUpdatedAtUtc >= originalUpdatedAtUtc);
        }

        [Fact]
        public void RemoveItem_RemovesOnlyRequestedItem()
        {
            var treatmentPlan = CreateTreatmentPlan();
            var keptItem = treatmentPlan.AddItem("Exam", null, 1, null, null, null, Guid.NewGuid());
            var removedItem = treatmentPlan.AddItem("Sealant", null, 1, null, "16", "O", Guid.NewGuid());
            var actorUserId = Guid.NewGuid();

            treatmentPlan.RemoveItem(removedItem.Id, actorUserId);

            Assert.Contains(treatmentPlan.Items, entry => entry.Id == keptItem.Id);
            Assert.DoesNotContain(treatmentPlan.Items, entry => entry.Id == removedItem.Id);
            Assert.Equal(actorUserId, treatmentPlan.LastUpdatedByUserId);
        }

        [Fact]
        public void ChangeStatus_UpdatesStatusAndMetadata()
        {
            var treatmentPlan = CreateTreatmentPlan();
            var proposedByUserId = Guid.NewGuid();
            var acceptedByUserId = Guid.NewGuid();

            var proposedChanged = treatmentPlan.ChangeStatus(TreatmentPlanStatus.Proposed, proposedByUserId);
            var acceptedChanged = treatmentPlan.ChangeStatus(TreatmentPlanStatus.Accepted, acceptedByUserId);

            Assert.True(proposedChanged);
            Assert.True(acceptedChanged);
            Assert.Equal(TreatmentPlanStatus.Accepted, treatmentPlan.Status);
            Assert.Equal(acceptedByUserId, treatmentPlan.LastUpdatedByUserId);
        }

        [Fact]
        public void ChangeStatus_RejectsInvalidTransition()
        {
            var treatmentPlan = CreateTreatmentPlan();

            var exception = Assert.Throws<InvalidOperationException>(() =>
                treatmentPlan.ChangeStatus(TreatmentPlanStatus.Accepted, Guid.NewGuid()));

            Assert.Contains("Draft -> Proposed", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void AcceptedPlan_BlocksItemMutations()
        {
            var treatmentPlan = CreateTreatmentPlan();
            treatmentPlan.ChangeStatus(TreatmentPlanStatus.Proposed, Guid.NewGuid());
            treatmentPlan.ChangeStatus(TreatmentPlanStatus.Accepted, Guid.NewGuid());

            var addException = Assert.Throws<InvalidOperationException>(() =>
                treatmentPlan.AddItem("Exam", null, 1, null, null, null, Guid.NewGuid()));
            var removeException = Assert.Throws<InvalidOperationException>(() =>
                treatmentPlan.RemoveItem(Guid.NewGuid(), Guid.NewGuid()));

            Assert.Contains("read-only", addException.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("read-only", removeException.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void TreatmentPlanMutations_DoNotChangeTenantOrPatientOwnership()
        {
            var treatmentPlan = CreateTreatmentPlan();
            var originalTenantId = treatmentPlan.TenantId;
            var originalPatientId = treatmentPlan.PatientId;
            var item = treatmentPlan.AddItem("Exam", "Diagnostics", 1, null, null, null, Guid.NewGuid());
            treatmentPlan.ChangeStatus(TreatmentPlanStatus.Proposed, Guid.NewGuid());
            treatmentPlan.RemoveItem(item.Id, Guid.NewGuid());

            Assert.Equal(originalTenantId, treatmentPlan.TenantId);
            Assert.Equal(originalPatientId, treatmentPlan.PatientId);
        }

        private static TreatmentPlan CreateTreatmentPlan()
        {
            return new TreatmentPlan(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid());
        }
    }
}
