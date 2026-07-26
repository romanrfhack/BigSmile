namespace BigSmile.Application.Interfaces.Security
{
    internal static class PatientPortalPassword
    {
        public static void Validate(
            string? password,
            IPatientPortalAuthenticationSettings settings)
        {
            ArgumentNullException.ThrowIfNull(settings);

            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException(
                    "Patient portal password is required.",
                    nameof(password));
            }

            if (password.Length < settings.MinimumPasswordLength ||
                password.Length > settings.MaximumPasswordLength)
            {
                throw new ArgumentException(
                    $"Patient portal password must contain between {settings.MinimumPasswordLength} and {settings.MaximumPasswordLength} characters.",
                    nameof(password));
            }

            if (password.Any(char.IsControl))
            {
                throw new ArgumentException(
                    "Patient portal password cannot contain control characters.",
                    nameof(password));
            }
        }
    }
}
