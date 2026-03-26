
namespace IdeaCollectionSystem.Service.Interfaces
{
	public interface IExportService
	{
		Task<byte[]> ExportIdeasToCsvAsync();
		Task<byte[]> ExportDocumentsToZipAsync();
		Task<byte[]> ExportIdeasBySubmissionAsync(Guid submissionId);
		Task<byte[]> ExportDocumentsBySubmissionToZipAsync(Guid submissionId);
	}
}