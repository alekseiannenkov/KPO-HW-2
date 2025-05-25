using FileAnalisysService.Models;
using Microsoft.EntityFrameworkCore;

namespace FileAnalisysService.Data;

public class AnalysisDbContext : DbContext
{
    public AnalysisDbContext(DbContextOptions<AnalysisDbContext> options) : base(options)
    {
    }

    public DbSet<FileAnalysisResult> AnalysisResults { get; set; }
    public DbSet<SimilarityMatch> SimilarityMatches { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FileAnalysisResult>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FileId).IsRequired();
            entity.Property(e => e.FileName).IsRequired();
            entity.Property(e => e.ParagraphCount).IsRequired();
            entity.Property(e => e.WordCount).IsRequired();
            entity.Property(e => e.CharacterCount).IsRequired();
            entity.Property(e => e.AnalysisDate).IsRequired();
            entity.Property(e => e.WordCloudPath).IsRequired(false);

            entity.HasIndex(e => e.FileId);

            entity.HasMany(e => e.SimilarityMatches)
                  .WithOne(e => e.FileAnalysisResult)
                  .HasForeignKey(e => e.FileAnalysisResultId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SimilarityMatch>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.MatchedFileId).IsRequired();
            entity.Property(e => e.MatchedFileName).IsRequired();
            entity.Property(e => e.SimilarityPercentage).IsRequired();

            entity.HasIndex(e => e.MatchedFileId);
        });
    }
}