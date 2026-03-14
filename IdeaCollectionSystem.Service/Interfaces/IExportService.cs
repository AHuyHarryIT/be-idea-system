namespace IdeaCollectionSystem.Service.Interfaces
{
	public interface IExportService
	{
		Task<byte[]> ExportIdeasToCsvAsync();
		Task<byte[]> ExportDocumentsToZipAsync();
	}
}