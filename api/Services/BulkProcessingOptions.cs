namespace RXNT.API.Services
{
    public class BulkProcessingOptions
    {
        public string UploadPath { get; set; } = string.Empty;
        public int BatchSize { get; set; } = 100;
        public int MaxFileSizeMb { get; set; } = 100;
    }
}


