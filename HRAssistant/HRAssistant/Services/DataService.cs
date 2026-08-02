using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Extensions.VectorData;
using UglyToad.PdfPig;

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

        public async Task IngestDocumentAsync(IFormFile file)
        {
            List<string> paragraphs;
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            using var stream = file.OpenReadStream();
            paragraphs = extension switch
            {
                ".docx" => ExtractParagraphsFromDocx(stream),
                ".pdf" => ExtractParagraphsFromPdf(stream),
                _ => throw new NotSupportedException("Unsupported file type.")
            };

            var chunks = MergeParagraphs(paragraphs);

            await _collection.EnsureCollectionExistsAsync();

            foreach (var chunk in chunks)
            {
                var embedding = await _embeddingGenerator.GenerateVectorAsync(chunk);

                await _collection.UpsertAsync(new DocumentChunk
                {
                    FileName = file.FileName,
                    Content = chunk,
                    Embedding = embedding
                });
            }
        }

        private List<string> ExtractParagraphsFromDocx(Stream stream)
        {
            using var document = WordprocessingDocument.Open(stream, false);

            return document.MainDocumentPart!
                .Document!
                .Body!
                .Elements<Paragraph>()
                .Select(p => string.Join(" ",
                    p.Descendants<Text>().Select(t => t.Text)))
                .Select(p => p.Trim())
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .ToList();
        }

        private List<string> ExtractParagraphsFromPdf(Stream stream)
        {
            using var document = PdfDocument.Open(stream);

            var paragraphs = new List<string>();

            foreach (var page in document.GetPages())
            {
                var lines = page.Text.Split(
                    new[] { "\r\n", "\n" },
                    StringSplitOptions.RemoveEmptyEntries);

                paragraphs.AddRange(
                    lines.Select(l => l.Trim())
                         .Where(l => !string.IsNullOrWhiteSpace(l)));
            }

            return paragraphs;
        }

        private List<string> MergeParagraphs(List<string> paragraphs, int targetWords = 200)
        {
            var chunks = new List<string>();

            var currentChunk = new List<string>();
            var currentWordCount = 0;

            foreach (var paragraph in paragraphs)
            {
                int words = paragraph.Split(
                    ' ',
                    StringSplitOptions.RemoveEmptyEntries).Length;

                if (currentWordCount > 0 &&
                    currentWordCount + words > targetWords)
                {
                    chunks.Add(string.Join(
                        Environment.NewLine + Environment.NewLine,
                        currentChunk));

                    currentChunk.Clear();
                    currentWordCount = 0;
                }

                currentChunk.Add(paragraph);
                currentWordCount += words;
            }

            if (currentChunk.Count > 0)
            {
                chunks.Add(string.Join(
                    Environment.NewLine + Environment.NewLine,
                    currentChunk));
            }

            return chunks;
        }
    }
}
