using Microsoft.Extensions.VectorData;
using System.ComponentModel;

namespace HRAssistant.Plugins
{
    public class PolicyEnquiry
    {
        private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;
        private readonly VectorStoreCollection<string, DocumentChunk> _collection;

        public PolicyEnquiry(IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator, VectorStoreCollection<string, DocumentChunk> collection)
        {
            _embeddingGenerator = embeddingGenerator;
            _collection = collection;
        }

        [KernelFunction("search_policy"), Description("Search companies policy document to fetch company related information including travel plans, compensation, hierarchy, company goals, ongoing projects and many more.")]
        public async Task<VectorSearchResult<DocumentChunk>?> SearchPolicy(string query)
        {
            var queryEmbedding = await _embeddingGenerator.GenerateVectorAsync(query);
            var vectorSearchOptions = new VectorSearchOptions<DocumentChunk>()
            {
                VectorProperty = x => x.Embedding
            };

            await foreach (var result in _collection.SearchAsync(queryEmbedding, top: 1, options: vectorSearchOptions))
                return result;

            return null;
        }
    }
}
