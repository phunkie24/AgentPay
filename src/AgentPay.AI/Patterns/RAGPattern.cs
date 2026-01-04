using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgentPay.AI.Services;

namespace AgentPay.AI.Patterns;

/// <summary>
/// Pattern 11: Retrieval-Augmented Generation (RAG)
/// Retrieves relevant information before generating a response
/// </summary>
public class RAGPattern
{
    private readonly ILLMService _llm;

    public RAGPattern(ILLMService llm)
    {
        _llm = llm;
    }

    public async Task<RAGResult> ExecuteAsync(
        string query,
        List<Document> knowledgeBase,
        int topK = 3)
    {
        // Retrieve relevant documents
        var relevantDocs = await RetrieveRelevantDocumentsAsync(query, knowledgeBase, topK);

        // Generate response with context
        var response = await GenerateWithContextAsync(query, relevantDocs);

        return new RAGResult
        {
            Query = query,
            RetrievedDocuments = relevantDocs,
            GeneratedResponse = response,
            Timestamp = DateTime.UtcNow
        };
    }

    private async Task<List<RelevantDocument>> RetrieveRelevantDocumentsAsync(
        string query,
        List<Document> knowledgeBase,
        int topK)
    {
        // In a real implementation, this would use vector similarity search
        // For now, using LLM to assess relevance
        var relevantDocs = new List<RelevantDocument>();

        foreach (var doc in knowledgeBase)
        {
            var relevancePrompt = $"""
            Query: {query}
            Document: {doc.Content}

            Rate the relevance of this document to the query on a scale of 0-1.
            Respond with just the number.
            """;

            var response = await _llm.GenerateAsync(relevancePrompt, new LLMOptions
            {
                Temperature = 0.1,
                MaxTokens = 10
            });

            if (double.TryParse(response.Text.Trim(), out double score))
            {
                relevantDocs.Add(new RelevantDocument
                {
                    Document = doc,
                    RelevanceScore = score
                });
            }
        }

        return relevantDocs
            .OrderByDescending(d => d.RelevanceScore)
            .Take(topK)
            .ToList();
    }

    private async Task<string> GenerateWithContextAsync(
        string query,
        List<RelevantDocument> relevantDocs)
    {
        var context = string.Join("\n\n", relevantDocs.Select((doc, i) =>
            $"[Document {i + 1}]\n{doc.Document.Content}"));

        var prompt = $"""
        Use the following context to answer the question. If the answer isn't in the context, say so.

        Context:
        {context}

        Question: {query}

        Answer:
        """;

        var response = await _llm.GenerateAsync(prompt, new LLMOptions
        {
            Temperature = 0.3,
            MaxTokens = 500
        });

        return response.Text.Trim();
    }
}

public class Document
{
    public string Id { get; set; }
    public string Content { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class RelevantDocument
{
    public Document Document { get; set; }
    public double RelevanceScore { get; set; }
}

public class RAGResult
{
    public string Query { get; set; }
    public List<RelevantDocument> RetrievedDocuments { get; set; }
    public string GeneratedResponse { get; set; }
    public DateTime Timestamp { get; set; }
}
