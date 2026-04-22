using BigSmile.Domain.Entities;

namespace BigSmile.UnitTests.TreatmentQuotes
{
    public class TreatmentQuoteTests
    {
        [Fact]
        public void CreateTreatmentQuote_StartsAsDraftWithFixedCurrencyAndSnapshotsPlanItems()
        {
            var actorUserId = Guid.NewGuid();
            var treatmentPlan = CreateTreatmentPlan(actorUserId);
            var sourceItem = treatmentPlan.AddItem(
                "Composite restoration",
                "Restorative",
                2,
                "Two-surface restoration.",
                "11",
                "O",
                actorUserId);

            var treatmentQuote = new TreatmentQuote(
                treatmentPlan.TenantId,
                treatmentPlan.PatientId,
                treatmentPlan.Id,
                treatmentPlan.Items,
                actorUserId);

            Assert.Equal(TreatmentQuoteStatus.Draft, treatmentQuote.Status);
            Assert.Equal("MXN", treatmentQuote.CurrencyCode);
            Assert.Equal(treatmentPlan.TenantId, treatmentQuote.TenantId);
            Assert.Equal(treatmentPlan.PatientId, treatmentQuote.PatientId);
            Assert.Equal(treatmentPlan.Id, treatmentQuote.TreatmentPlanId);

            var item = Assert.Single(treatmentQuote.Items);
            Assert.Equal(sourceItem.Id, item.SourceTreatmentPlanItemId);
            Assert.Equal(sourceItem.Title, item.Title);
            Assert.Equal(sourceItem.Category, item.Category);
            Assert.Equal(sourceItem.Quantity, item.Quantity);
            Assert.Equal(sourceItem.Notes, item.Notes);
            Assert.Equal(sourceItem.ToothCode, item.ToothCode);
            Assert.Equal(sourceItem.SurfaceCode, item.SurfaceCode);
            Assert.Equal(0m, item.UnitPrice);
            Assert.Equal(0m, item.GetLineTotal());
            Assert.Equal(0m, treatmentQuote.GetTotal());
        }

        [Fact]
        public void CreateTreatmentQuote_RejectsEmptySnapshot()
        {
            var treatmentPlan = CreateTreatmentPlan(Guid.NewGuid());

            var exception = Assert.Throws<ArgumentException>(() =>
                new TreatmentQuote(
                    treatmentPlan.TenantId,
                    treatmentPlan.PatientId,
                    treatmentPlan.Id,
                    treatmentPlan.Items,
                    Guid.NewGuid()));

            Assert.Contains("at least one treatment plan item", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void UpdateItemUnitPrice_RejectsNegativePrice()
        {
            var treatmentQuote = CreateTreatmentQuote();
            var item = Assert.Single(treatmentQuote.Items);

            var exception = Assert.Throws<ArgumentException>(() =>
                treatmentQuote.UpdateItemUnitPrice(item.Id, -1m, Guid.NewGuid()));

            Assert.Contains("greater than or equal to zero", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void UpdateItemUnitPrice_UpdatesTotalsAndMetadata()
        {
            var treatmentQuote = CreateTreatmentQuote();
            var item = Assert.Single(treatmentQuote.Items);
            var actorUserId = Guid.NewGuid();
            var originalUpdatedAtUtc = treatmentQuote.LastUpdatedAtUtc;

            var changed = treatmentQuote.UpdateItemUnitPrice(item.Id, 450m, actorUserId);

            Assert.True(changed);
            Assert.Equal(450m, item.UnitPrice);
            Assert.Equal(450m * item.Quantity, item.GetLineTotal());
            Assert.Equal(item.GetLineTotal(), treatmentQuote.GetTotal());
            Assert.Equal(actorUserId, treatmentQuote.LastUpdatedByUserId);
            Assert.True(treatmentQuote.LastUpdatedAtUtc >= originalUpdatedAtUtc);
        }

        [Fact]
        public void UpdateItemUnitPrice_RejectsZeroPrice_WhenQuoteIsProposed()
        {
            var treatmentQuote = CreateTreatmentQuote();
            var item = Assert.Single(treatmentQuote.Items);
            treatmentQuote.UpdateItemUnitPrice(item.Id, 250m, Guid.NewGuid());
            treatmentQuote.ChangeStatus(TreatmentQuoteStatus.Proposed, Guid.NewGuid());

            var exception = Assert.Throws<InvalidOperationException>(() =>
                treatmentQuote.UpdateItemUnitPrice(item.Id, 0m, Guid.NewGuid()));

            Assert.Contains("greater than zero", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void ChangeStatus_RejectsProposedWhenAnyItemHasNoPositivePrice()
        {
            var treatmentQuote = CreateTreatmentQuote();

            var exception = Assert.Throws<InvalidOperationException>(() =>
                treatmentQuote.ChangeStatus(TreatmentQuoteStatus.Proposed, Guid.NewGuid()));

            Assert.Contains("greater than zero", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void ChangeStatus_AllowsAcceptedWhenProposedQuoteKeepsPositivePrices()
        {
            var treatmentQuote = CreateTreatmentQuote();
            var item = Assert.Single(treatmentQuote.Items);
            treatmentQuote.UpdateItemUnitPrice(item.Id, 250m, Guid.NewGuid());
            treatmentQuote.ChangeStatus(TreatmentQuoteStatus.Proposed, Guid.NewGuid());

            var changed = treatmentQuote.ChangeStatus(TreatmentQuoteStatus.Accepted, Guid.NewGuid());

            Assert.True(changed);
            Assert.Equal(TreatmentQuoteStatus.Accepted, treatmentQuote.Status);
        }

        [Fact]
        public void ChangeStatus_RejectsAcceptedWhenProposedQuoteContainsNonPositivePrice()
        {
            var treatmentQuote = CreateTreatmentQuote();
            var item = Assert.Single(treatmentQuote.Items);
            treatmentQuote.UpdateItemUnitPrice(item.Id, 250m, Guid.NewGuid());
            treatmentQuote.ChangeStatus(TreatmentQuoteStatus.Proposed, Guid.NewGuid());
            ForceUnitPriceForTest(item, 0m);

            var exception = Assert.Throws<InvalidOperationException>(() =>
                treatmentQuote.ChangeStatus(TreatmentQuoteStatus.Accepted, Guid.NewGuid()));

            Assert.Contains("greater than zero", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void ChangeStatus_RejectsInvalidTransition()
        {
            var treatmentQuote = CreateTreatmentQuote();

            var exception = Assert.Throws<InvalidOperationException>(() =>
                treatmentQuote.ChangeStatus(TreatmentQuoteStatus.Accepted, Guid.NewGuid()));

            Assert.Contains("Draft -> Proposed", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void AcceptedQuote_BlocksPriceUpdatesAndFurtherStatusChanges()
        {
            var treatmentQuote = CreateTreatmentQuote();
            var item = Assert.Single(treatmentQuote.Items);
            treatmentQuote.UpdateItemUnitPrice(item.Id, 250m, Guid.NewGuid());
            treatmentQuote.ChangeStatus(TreatmentQuoteStatus.Proposed, Guid.NewGuid());
            treatmentQuote.ChangeStatus(TreatmentQuoteStatus.Accepted, Guid.NewGuid());

            var priceException = Assert.Throws<InvalidOperationException>(() =>
                treatmentQuote.UpdateItemUnitPrice(item.Id, 300m, Guid.NewGuid()));
            var statusException = Assert.Throws<InvalidOperationException>(() =>
                treatmentQuote.ChangeStatus(TreatmentQuoteStatus.Draft, Guid.NewGuid()));

            Assert.Contains("read-only", priceException.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Draft -> Proposed", statusException.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void TreatmentQuoteMutations_DoNotChangeTenantPatientOrTreatmentPlanOwnership()
        {
            var treatmentQuote = CreateTreatmentQuote();
            var item = Assert.Single(treatmentQuote.Items);
            var originalTenantId = treatmentQuote.TenantId;
            var originalPatientId = treatmentQuote.PatientId;
            var originalTreatmentPlanId = treatmentQuote.TreatmentPlanId;

            treatmentQuote.UpdateItemUnitPrice(item.Id, 150m, Guid.NewGuid());
            treatmentQuote.ChangeStatus(TreatmentQuoteStatus.Proposed, Guid.NewGuid());
            treatmentQuote.ChangeStatus(TreatmentQuoteStatus.Accepted, Guid.NewGuid());

            Assert.Equal(originalTenantId, treatmentQuote.TenantId);
            Assert.Equal(originalPatientId, treatmentQuote.PatientId);
            Assert.Equal(originalTreatmentPlanId, treatmentQuote.TreatmentPlanId);
        }

        [Fact]
        public void TreatmentQuote_RemainsSnapshotOnly_WhenTreatmentPlanChangesAfterQuoteCreation()
        {
            var actorUserId = Guid.NewGuid();
            var treatmentPlan = CreateTreatmentPlan(actorUserId);
            var originalItem = treatmentPlan.AddItem(
                "Composite restoration",
                "Restorative",
                2,
                "Two-surface restoration.",
                "11",
                "O",
                actorUserId);

            var treatmentQuote = new TreatmentQuote(
                treatmentPlan.TenantId,
                treatmentPlan.PatientId,
                treatmentPlan.Id,
                treatmentPlan.Items,
                actorUserId);

            treatmentPlan.AddItem("Crown", "Prosthodontics", 1, null, "21", null, actorUserId);
            treatmentPlan.RemoveItem(originalItem.Id, actorUserId);

            var snapshotItem = Assert.Single(treatmentQuote.Items);
            Assert.Single(treatmentPlan.Items);
            Assert.Equal(originalItem.Id, snapshotItem.SourceTreatmentPlanItemId);
            Assert.Equal("Composite restoration", snapshotItem.Title);
            Assert.DoesNotContain(treatmentPlan.Items, item => item.Id == snapshotItem.SourceTreatmentPlanItemId);
        }

        private static TreatmentQuote CreateTreatmentQuote()
        {
            var actorUserId = Guid.NewGuid();
            var treatmentPlan = CreateTreatmentPlan(actorUserId);
            treatmentPlan.AddItem("Exam", "Diagnostics", 1, null, null, null, actorUserId);

            return new TreatmentQuote(
                treatmentPlan.TenantId,
                treatmentPlan.PatientId,
                treatmentPlan.Id,
                treatmentPlan.Items,
                actorUserId);
        }

        private static TreatmentPlan CreateTreatmentPlan(Guid actorUserId)
        {
            return new TreatmentPlan(
                Guid.NewGuid(),
                Guid.NewGuid(),
                actorUserId);
        }

        private static void ForceUnitPriceForTest(TreatmentQuoteItem item, decimal unitPrice)
        {
            var property = typeof(TreatmentQuoteItem).GetProperty(
                nameof(TreatmentQuoteItem.UnitPrice),
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)
                ?? throw new InvalidOperationException("TreatmentQuoteItem.UnitPrice property was not found for the test.");

            var setter = property.GetSetMethod(nonPublic: true)
                ?? throw new InvalidOperationException("TreatmentQuoteItem.UnitPrice setter was not found for the test.");

            setter.Invoke(item, new object[] { unitPrice });
        }
    }
}
