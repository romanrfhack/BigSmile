using BigSmile.Domain.Entities;

namespace BigSmile.UnitTests.BillingDocuments
{
    public class BillingDocumentTests
    {
        [Fact]
        public void Constructor_CopiesQuoteSnapshotIntoBillingDocument()
        {
            var actorUserId = Guid.NewGuid();
            var treatmentPlan = new TreatmentPlan(Guid.NewGuid(), Guid.NewGuid(), actorUserId);
            treatmentPlan.AddItem(
                "Composite restoration",
                "Restorative",
                2,
                "Upper right molar",
                "16",
                "O",
                actorUserId);

            var treatmentQuote = new TreatmentQuote(
                Guid.NewGuid(),
                treatmentPlan.PatientId,
                treatmentPlan.Id,
                treatmentPlan.Items,
                actorUserId);

            treatmentQuote.UpdateItemUnitPrice(treatmentQuote.Items.Single().Id, 450m, actorUserId);
            treatmentQuote.ChangeStatus(TreatmentQuoteStatus.Proposed, actorUserId);
            treatmentQuote.ChangeStatus(TreatmentQuoteStatus.Accepted, actorUserId);
            var treatmentQuoteItemId = treatmentQuote.Items.Single().Id;

            var billingDocument = new BillingDocument(
                treatmentQuote.TenantId,
                treatmentQuote.PatientId,
                treatmentQuote.Id,
                treatmentQuote.CurrencyCode,
                treatmentQuote.Items,
                actorUserId);

            Assert.Equal(treatmentQuote.TenantId, billingDocument.TenantId);
            Assert.Equal(treatmentQuote.PatientId, billingDocument.PatientId);
            Assert.Equal(treatmentQuote.Id, billingDocument.TreatmentQuoteId);
            Assert.Equal("MXN", billingDocument.CurrencyCode);
            Assert.Equal(BillingDocumentStatus.Draft, billingDocument.Status);
            Assert.Equal(900m, billingDocument.TotalAmount);

            var snapshotItem = Assert.Single(billingDocument.Items);
            Assert.Equal(treatmentQuoteItemId, snapshotItem.SourceTreatmentQuoteItemId);
            Assert.Equal("Composite restoration", snapshotItem.Title);
            Assert.Equal("Restorative", snapshotItem.Category);
            Assert.Equal(2, snapshotItem.Quantity);
            Assert.Equal("Upper right molar", snapshotItem.Notes);
            Assert.Equal("16", snapshotItem.ToothCode);
            Assert.Equal("O", snapshotItem.SurfaceCode);
            Assert.Equal(450m, snapshotItem.UnitPrice);
            Assert.Equal(900m, snapshotItem.LineTotal);
            Assert.Equal(actorUserId, snapshotItem.CreatedByUserId);
        }

        [Fact]
        public void ChangeStatus_AllowsDraftToIssuedAndCapturesIssueMetadata()
        {
            var actorUserId = Guid.NewGuid();
            var billingDocument = CreateBillingDocument(actorUserId);

            var changed = billingDocument.ChangeStatus(BillingDocumentStatus.Issued, actorUserId);

            Assert.True(changed);
            Assert.Equal(BillingDocumentStatus.Issued, billingDocument.Status);
            Assert.Equal(actorUserId, billingDocument.LastUpdatedByUserId);
            Assert.Equal(actorUserId, billingDocument.IssuedByUserId);
            Assert.NotNull(billingDocument.IssuedAtUtc);
            Assert.Equal(billingDocument.IssuedAtUtc, billingDocument.LastUpdatedAtUtc);
        }

        [Fact]
        public void IssuedBillingDocument_BlocksFurtherStatusChanges()
        {
            var billingDocument = CreateBillingDocument(Guid.NewGuid());
            billingDocument.ChangeStatus(BillingDocumentStatus.Issued, Guid.NewGuid());

            var exception = Assert.Throws<InvalidOperationException>(() =>
                billingDocument.ChangeStatus(BillingDocumentStatus.Issued, Guid.NewGuid()));

            Assert.Contains("read-only", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void BillingDocument_RemainsSnapshotOnly_WhenSourceQuoteChangesLater()
        {
            var actorUserId = Guid.NewGuid();
            var treatmentPlan = new TreatmentPlan(Guid.NewGuid(), Guid.NewGuid(), actorUserId);
            treatmentPlan.AddItem("Exam", "Diagnostics", 1, null, null, null, actorUserId);

            var treatmentQuote = new TreatmentQuote(
                Guid.NewGuid(),
                treatmentPlan.PatientId,
                treatmentPlan.Id,
                treatmentPlan.Items,
                actorUserId);

            var quoteItemId = treatmentQuote.Items.Single().Id;
            treatmentQuote.UpdateItemUnitPrice(quoteItemId, 350m, actorUserId);

            var billingDocument = new BillingDocument(
                treatmentQuote.TenantId,
                treatmentQuote.PatientId,
                treatmentQuote.Id,
                treatmentQuote.CurrencyCode,
                treatmentQuote.Items,
                actorUserId);

            var billingSnapshotUnitPrice = billingDocument.Items.Single().UnitPrice;

            treatmentQuote.UpdateItemUnitPrice(quoteItemId, 425m, actorUserId);

            Assert.Equal(425m, treatmentQuote.Items.Single().UnitPrice);
            Assert.Equal(billingSnapshotUnitPrice, billingDocument.Items.Single().UnitPrice);
            Assert.Equal(350m, billingDocument.Items.Single().LineTotal);
            Assert.Equal(350m, billingDocument.TotalAmount);
        }

        private static BillingDocument CreateBillingDocument(Guid actorUserId)
        {
            var treatmentPlan = new TreatmentPlan(Guid.NewGuid(), Guid.NewGuid(), actorUserId);
            treatmentPlan.AddItem("Exam", "Diagnostics", 1, null, null, null, actorUserId);

            var treatmentQuote = new TreatmentQuote(
                Guid.NewGuid(),
                treatmentPlan.PatientId,
                treatmentPlan.Id,
                treatmentPlan.Items,
                actorUserId);

            treatmentQuote.UpdateItemUnitPrice(treatmentQuote.Items.Single().Id, 350m, actorUserId);
            treatmentQuote.ChangeStatus(TreatmentQuoteStatus.Proposed, actorUserId);
            treatmentQuote.ChangeStatus(TreatmentQuoteStatus.Accepted, actorUserId);

            return new BillingDocument(
                treatmentQuote.TenantId,
                treatmentQuote.PatientId,
                treatmentQuote.Id,
                treatmentQuote.CurrencyCode,
                treatmentQuote.Items,
                actorUserId);
        }
    }
}
