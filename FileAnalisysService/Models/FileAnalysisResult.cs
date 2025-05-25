namespace FileAnalisysService.Models;

public class FileAnalysisResult
{
    public Guid Id { get; set; } // PK
    public Guid FileId { get; set; } // Index
    public string? FileName { get; set; }
    public int ParagraphCount { get; set; }
    public int WordCount { get; set; }
    public int CharacterCount { get; set; }
    public DateTime AnalysisDate { get; set; }
    public List<SimilarityMatch> SimilarityMatches { get; set; } = new List<SimilarityMatch>();
    public string? WordCloudPath { get; set; }
}