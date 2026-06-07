namespace HRAssistant.Services
{
    public interface IDataService
    {
        Task IngestDocumentAsync(string filePath);
    }
}
