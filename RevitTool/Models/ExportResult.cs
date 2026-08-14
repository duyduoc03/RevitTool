namespace RevitTool.Models
{
    public sealed class ExportResult
    {
        public bool Success { get; init; }

        public string? FilePath { get; init; }

        public required string Message { get; init; }
    }
}
