namespace BigSmile.Infrastructure.Options
{
    public sealed class PatientDocumentStorageOptions
    {
        public const string SectionName = "PatientDocumentStorage";

        public string? RootPath { get; set; }
    }
}
