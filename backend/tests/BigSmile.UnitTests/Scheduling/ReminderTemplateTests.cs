using BigSmile.Domain.Entities;

namespace BigSmile.UnitTests.Scheduling
{
    public sealed class ReminderTemplateTests
    {
        [Fact]
        public void Constructor_TrimsNameAndBodyAndStartsActive()
        {
            var tenantId = Guid.NewGuid();
            var actorUserId = Guid.NewGuid();

            var template = new ReminderTemplate(
                tenantId,
                " Confirmacion ",
                " Hola {{patientName}}. ",
                actorUserId);

            Assert.Equal(tenantId, template.TenantId);
            Assert.Equal("Confirmacion", template.Name);
            Assert.Equal("Hola {{patientName}}.", template.Body);
            Assert.True(template.IsActive);
            Assert.Equal(actorUserId, template.CreatedByUserId);
            Assert.Null(template.UpdatedAtUtc);
            Assert.Null(template.DeactivatedAtUtc);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_BlocksEmptyName(string name)
        {
            Assert.Throws<ArgumentException>(() => new ReminderTemplate(
                Guid.NewGuid(),
                name,
                "Hola {{patientName}}.",
                Guid.NewGuid()));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_BlocksEmptyBody(string body)
        {
            Assert.Throws<ArgumentException>(() => new ReminderTemplate(
                Guid.NewGuid(),
                "Confirmacion",
                body,
                Guid.NewGuid()));
        }

        [Fact]
        public void Constructor_BlocksLongNameAndBody()
        {
            Assert.Throws<ArgumentException>(() => new ReminderTemplate(
                Guid.NewGuid(),
                new string('a', ReminderTemplate.NameMaxLength + 1),
                "Hola.",
                Guid.NewGuid()));

            Assert.Throws<ArgumentException>(() => new ReminderTemplate(
                Guid.NewGuid(),
                "Confirmacion",
                new string('a', ReminderTemplate.BodyMaxLength + 1),
                Guid.NewGuid()));
        }

        [Fact]
        public void Update_ChangesNameBodyAndMetadataWithoutChangingCreation()
        {
            var createdByUserId = Guid.NewGuid();
            var updatedByUserId = Guid.NewGuid();
            var createdAtUtc = new DateTime(2026, 4, 27, 8, 0, 0);
            var template = new ReminderTemplate(
                Guid.NewGuid(),
                "Original",
                "Original body",
                createdByUserId,
                createdAtUtc);

            template.Update(" Updated ", " Updated body ", updatedByUserId);

            Assert.Equal("Updated", template.Name);
            Assert.Equal("Updated body", template.Body);
            Assert.Equal(createdAtUtc, template.CreatedAtUtc);
            Assert.Equal(createdByUserId, template.CreatedByUserId);
            Assert.NotNull(template.UpdatedAtUtc);
            Assert.Equal(updatedByUserId, template.UpdatedByUserId);
            Assert.True(template.IsActive);
        }

        [Fact]
        public void Deactivate_SoftDeactivatesWithMetadataAndIsIdempotent()
        {
            var actorUserId = Guid.NewGuid();
            var template = new ReminderTemplate(
                Guid.NewGuid(),
                "Confirmacion",
                "Hola.",
                Guid.NewGuid());

            template.Deactivate(actorUserId);
            var deactivatedAtUtc = template.DeactivatedAtUtc;

            template.Deactivate(actorUserId);

            Assert.False(template.IsActive);
            Assert.Equal(actorUserId, template.DeactivatedByUserId);
            Assert.Equal(deactivatedAtUtc, template.DeactivatedAtUtc);
        }

        [Fact]
        public void Update_BlocksInactiveTemplates()
        {
            var template = new ReminderTemplate(
                Guid.NewGuid(),
                "Confirmacion",
                "Hola.",
                Guid.NewGuid());
            template.Deactivate(Guid.NewGuid());

            var exception = Assert.Throws<InvalidOperationException>(() =>
                template.Update("Updated", "Updated body", Guid.NewGuid()));

            Assert.Contains("inactive", exception.Message, StringComparison.OrdinalIgnoreCase);
        }
    }
}
