using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Extensions.VectorData;

namespace HRAssistant.Services
{
    public class DataService : IDataService
    {
        private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;
        private readonly VectorStoreCollection<string, DocumentChunk> _collection;

        public DataService(IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator, VectorStoreCollection<string, DocumentChunk> collection)
        {
            _embeddingGenerator = embeddingGenerator;
            _collection = collection;
        }

        public async Task IngestDocumentAsync(string filePath)
        {
            var text = filePath.EndsWith(".docx")
                        ? ExtractTextFromDocx(filePath)
                        : await File.ReadAllTextAsync(filePath);

            var fileName = Path.GetFileName(filePath);
            var chunks = ChunkText(text, chunkSize: 80);

            await _collection.EnsureCollectionExistsAsync();

            foreach (var chunk in chunks)
            {
                var embedding = await _embeddingGenerator.GenerateVectorAsync(chunk);
                var document = new DocumentChunk()
                {
                    Content = chunk,
                    FileName = fileName,
                    Embedding = embedding
                };

                await _collection.UpsertAsync(document);
            }
        }

        private static List<string> ChunkText(string text, int chunkSize)
        {
            var chunks = new List<string>();
            var words = text.Split(" ", StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < words.Length; i += chunkSize)
            {
                chunks.Add(string.Join(' ', words.Skip(i).Take(chunkSize)));
            }

            return chunks;
        }

        private static string ExtractTextFromDocx(string filePath)
        {
            using var doc = WordprocessingDocument.Open(filePath, false);
            var body = doc.MainDocumentPart.Document.Body;
            return string.Join(" ", body.Descendants<Text>().Select(t => t.Text));
        }
    }
}
