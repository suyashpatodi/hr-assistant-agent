namespace HRAssistant.Services
{
    public interface IDataService
    {
        Task IngestDocumentAsync(IFormFile file);
    }
}
