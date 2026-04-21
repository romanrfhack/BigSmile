using BigSmile.Application.Features.Odontograms.Dtos;
using BigSmile.Domain.Entities;

namespace BigSmile.UnitTests.Odontogram
{
    public class OdontogramMappingsTests
    {
        [Fact]
        public void ToDetailDto_OrdersFindingsHistoryNewestFirst()
        {
            var actorUserId = Guid.NewGuid();
            var odontogram = new Domain.Entities.Odontogram(Guid.NewGuid(), Guid.NewGuid(), actorUserId);

            var addedFinding = odontogram.AddSurfaceFinding("11", "O", OdontogramSurfaceFindingType.Caries, actorUserId);
            var addedEntry = odontogram.SurfaceFindingHistoryEntries.Single(entry =>
                entry.EntryType == OdontogramSurfaceFindingHistoryEntryType.FindingAdded &&
                entry.ReferenceFindingId == addedFinding.Id);
            SetChangedAt(addedEntry, new DateTime(2026, 4, 20, 10, 0, 0, DateTimeKind.Utc));

            odontogram.RemoveSurfaceFinding("11", "O", addedFinding.Id, actorUserId);
            var removedEntry = odontogram.SurfaceFindingHistoryEntries.Single(entry =>
                entry.EntryType == OdontogramSurfaceFindingHistoryEntryType.FindingRemoved &&
                entry.ReferenceFindingId == addedFinding.Id);
            SetChangedAt(removedEntry, new DateTime(2026, 4, 20, 11, 0, 0, DateTimeKind.Utc));

            var secondFinding = odontogram.AddSurfaceFinding("11", "O", OdontogramSurfaceFindingType.Sealant, actorUserId);
            var secondAddedEntry = odontogram.SurfaceFindingHistoryEntries.Single(entry =>
                entry.EntryType == OdontogramSurfaceFindingHistoryEntryType.FindingAdded &&
                entry.ReferenceFindingId == secondFinding.Id);
            SetChangedAt(secondAddedEntry, new DateTime(2026, 4, 20, 12, 0, 0, DateTimeKind.Utc));

            var dto = odontogram.ToDetailDto();

            Assert.Equal(
                new[]
                {
                    "FindingAdded",
                    "FindingRemoved",
                    "FindingAdded"
                },
                dto.FindingsHistory.Select(entry => entry.EntryType).ToArray());

            Assert.Equal(
                new[]
                {
                    "Sealant",
                    "Caries",
                    "Caries"
                },
                dto.FindingsHistory.Select(entry => entry.FindingType).ToArray());

            Assert.Equal(
                new[]
                {
                    "Finding added",
                    "Finding removed",
                    "Finding added"
                },
                dto.FindingsHistory.Select(entry => entry.Summary).ToArray());
        }

        private static void SetChangedAt(OdontogramSurfaceFindingHistoryEntry historyEntry, DateTime value)
        {
            var field = typeof(OdontogramSurfaceFindingHistoryEntry)
                .GetField($"<{nameof(OdontogramSurfaceFindingHistoryEntry.ChangedAtUtc)}>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;

            field.SetValue(historyEntry, value);
        }
    }
}
