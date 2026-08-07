namespace HRAssistant.Services
{
    public interface IDataService
    {
        Task IngestDocumentAsync(IFormFile file);
        Task<Employee?> GetEmployeeData(string email);
    }
}
