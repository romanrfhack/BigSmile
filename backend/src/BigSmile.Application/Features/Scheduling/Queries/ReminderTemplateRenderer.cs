using System.Globalization;
using System.Text.RegularExpressions;
using BigSmile.Domain.Entities;

namespace BigSmile.Application.Features.Scheduling.Queries
{
    internal sealed record ReminderTemplateRenderResult(
        string RenderedBody,
        IReadOnlyList<string> UnknownPlaceholders);

    internal static partial class ReminderTemplateRenderer
    {
        private static readonly StringComparer PlaceholderComparer = StringComparer.Ordinal;

        public static ReminderTemplateRenderResult Render(
            ReminderTemplate template,
            Appointment appointment,
            Tenant tenant,
            Branch branch)
        {
            var values = new Dictionary<string, string>(PlaceholderComparer)
            {
                ["patientName"] = appointment.Patient.FullName,
                ["appointmentDate"] = appointment.StartsAt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                ["appointmentTime"] = appointment.StartsAt.ToString("HH:mm", CultureInfo.InvariantCulture),
                ["branchName"] = branch.Name,
                ["tenantName"] = tenant.Name
            };
            var unknownPlaceholders = new SortedSet<string>(PlaceholderComparer);

            var renderedBody = PlaceholderRegex().Replace(template.Body, match =>
            {
                var placeholder = match.Groups["name"].Value;
                if (values.TryGetValue(placeholder, out var value))
                {
                    return value;
                }

                unknownPlaceholders.Add(placeholder);
                return match.Value;
            });

            return new ReminderTemplateRenderResult(renderedBody, unknownPlaceholders.ToArray());
        }

        [GeneratedRegex("\\{\\{\\s*(?<name>[A-Za-z][A-Za-z0-9_]*)\\s*\\}\\}")]
        private static partial Regex PlaceholderRegex();
    }
}
