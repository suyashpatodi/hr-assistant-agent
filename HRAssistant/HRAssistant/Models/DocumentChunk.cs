using Microsoft.Extensions.VectorData;

namespace HRAssistant.Models
{
    public class DocumentChunk
    {
        [VectorStoreKey]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [VectorStoreData]
        public string Content { get; set; } = default!;

        [VectorStoreData]
        public string FileName { get; set; } = default!;

        [VectorStoreVector(Dimensions: 384)]
        public ReadOnlyMemory<float> Embedding { get; set; }
    }
}
