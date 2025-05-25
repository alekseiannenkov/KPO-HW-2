using FileAnalisysService.Models;
using FileAnalisysService.Services;
using Microsoft.AspNetCore.Mvc;

namespace FileAnalisysService.Controllers;

[ApiController]
[Route("api/analysis")]
public class AnalysisController : ControllerBase
{
    private readonly IFileAnalysisService _fileAnalysisService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AnalysisController> _logger;
    private readonly string _fileStoringServiceUrl;

    public AnalysisController(
        IFileAnalysisService fileAnalysisService,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<AnalysisController> logger)
    {
        _fileAnalysisService = fileAnalysisService;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _fileStoringServiceUrl = configuration["FileStoringService:Url"] ?? "http://localhost:7002";
    }

    [HttpPost("analyze/{fileId:guid}")]
    [ProducesResponseType(typeof(FileAnalysisResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AnalyzeFile(Guid fileId)
    {
        try
        {
            _logger.LogInformation("Получен запрос на анализ файла с ID {FileId}", fileId);

            var httpClient = _httpClientFactory.CreateClient();
            _logger.LogInformation("Connecting to FileStoringService at: {Url}", _fileStoringServiceUrl);
            var fileMetadataResponse = await httpClient.GetAsync($"{_fileStoringServiceUrl}/api/files/metadata/{fileId}");

            if (!fileMetadataResponse.IsSuccessStatusCode)
            {
                _logger.LogWarning("Не удалось получить метаданные файла с ID {FileId}. " +
                                  "Код ответа: {StatusCode}", fileId, fileMetadataResponse.StatusCode);
                return NotFound($"Файл с ID {fileId} не найден");
            }

            var fileMetadata = await fileMetadataResponse.Content.ReadFromJsonAsync<FileRequest>();
            if (fileMetadata == null)
            {
                _logger.LogWarning("Получены некорректные метаданные для файла с ID {FileId}", fileId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Получены некорректные метаданные файла");
            }

            var fileContentResponse = await httpClient.GetAsync($"{_fileStoringServiceUrl}/api/files/{fileId}");
            if (!fileContentResponse.IsSuccessStatusCode)
            {
                _logger.LogWarning("Не удалось получить содержимое файла с ID {FileId}. " +
                                  "Код ответа: {StatusCode}", fileId, fileContentResponse.StatusCode);
                return NotFound($"Содержимое файла с ID {fileId} не найдено");
            }

            var fileContent = await fileContentResponse.Content.ReadAsByteArrayAsync();

            var analysisResult = await _fileAnalysisService.AnalyzeFileAsync(fileId, fileContent, fileMetadata.FileName);

            return Ok(analysisResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при анализе файла с ID {FileId}", fileId);
            return StatusCode(StatusCodes.Status500InternalServerError, $"Внутренняя ошибка сервера при анализе файла: {ex.Message}");
        }
    }

    [HttpGet("{analysisId:guid}")]
    [ProducesResponseType(typeof(FileAnalysisResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAnalysisResult(Guid analysisId)
    {
        try {
            var result = await _fileAnalysisService.GetAnalysisResultAsync(analysisId);
            if (result == null)
            {
                return NotFound($"Результат анализа с ID {analysisId} не найден");
            }

            return Ok(result);
        } catch (Exception ex) {
            _logger.LogError(ex, "Ошибка при получении результата анализа с ID {AnalysisId}", analysisId);
            return StatusCode(StatusCodes.Status500InternalServerError, "Внутренняя ошибка сервера при получении результата анализа");
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<FileAnalysisResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllAnalysisResults()
    {
        try
        {
            var results = await _fileAnalysisService.GetAllAnalysisResultsAsync();
            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении всех результатов анализа");
            return StatusCode(StatusCodes.Status500InternalServerError, "Внутренняя ошибка сервера при получении всех результатов анализа");
        }
    }

    [HttpGet("wordcloud/{fileId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GenerateWordCloud(Guid fileId)
    {
        try
        {
            _logger.LogInformation("Получен запрос на генерацию облака слов для файла с ID {FileId}", fileId);
            var existingAnalysis = await _fileAnalysisService.GetAnalysisResultAsyncByFileId(fileId);
            if (existingAnalysis != null)
            {
                _logger.LogInformation("Найден существующий анализ для файла с ID {FileId}", fileId);
                var existingWordCloudResult = await _fileAnalysisService.GetWordCloudImageAsync(existingAnalysis.Id);
                if (existingWordCloudResult != null) {
                    _logger.LogInformation("Облако слов для файла с ID {FileId} уже существует", fileId);
                    return File(existingWordCloudResult, "image/png");
                }
            }

            var httpClient = _httpClientFactory.CreateClient();
            var fileMetadataResponse = await httpClient.GetAsync($"{_fileStoringServiceUrl}/api/files/metadata/{fileId}");
            if (!fileMetadataResponse.IsSuccessStatusCode)
            {
                return NotFound($"Файл с ID {fileId} не найден");
            }

            var fileMetadata = await fileMetadataResponse.Content.ReadFromJsonAsync<FileRequest>();
            if (fileMetadata == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Получены некорректные метаданные файла");
            }

            var fileContentResponse = await httpClient.GetAsync($"{_fileStoringServiceUrl}/api/files/{fileId}");
            if (!fileContentResponse.IsSuccessStatusCode)
            {
                return NotFound($"Содержимое файла с ID {fileId} не найдено");
            }

            var fileContent = await fileContentResponse.Content.ReadAsByteArrayAsync();
            var wordCloudResult = await _fileAnalysisService.GenerateWordCloudAsync(fileContent, fileMetadata.FileName);

            return File(wordCloudResult.imageData, "image/png");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при генерации облака слов для файла с ID {FileId}", fileId);
            return StatusCode(StatusCodes.Status500InternalServerError, "Внутренняя ошибка сервера при генерации облака слов");
        }
    }

    [HttpDelete("{analysisId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteAnalysis(Guid analysisId)
    {
        try
        {
            _logger.LogInformation("Получен запрос на удаление результата анализа с ID {AnalysisId}", analysisId);

            var isDeleted = await _fileAnalysisService.DeleteAnalysisAsync(analysisId);
            if (!isDeleted)
            {
                return NotFound($"Результат анализа с ID {analysisId} не найден");
            }

            return Ok($"Результат анализа с ID {analysisId} успешно удален");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении результата анализа с ID {AnalysisId}", analysisId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                $"Внутренняя ошибка сервера при удалении результата анализа: {ex.Message}");
        }
    }
}