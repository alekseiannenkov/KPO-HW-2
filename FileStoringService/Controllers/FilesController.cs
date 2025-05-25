using FileStoringService.Models;
using FileStoringService.Services;
using Microsoft.AspNetCore.Mvc;

namespace FileStoringService.Controllers;

[ApiController]
[Route("api/files")]
public class FilesController : ControllerBase
{
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<FilesController> _logger;

    public FilesController(IFileStorageService fileStorageService, ILogger<FilesController> logger)
    {
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    [HttpPost("upload")]
    [ProducesResponseType(typeof(FileMetadata), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UploadFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("Файл не был предоставлен или пуст");
        }

        if (!file.ContentType.Equals("text/plain", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("Поддерживаются только файлы в формате .txt");
        }

        try
        {
            var metadata = await _fileStorageService.SaveFileAsync(file);
            return Ok(metadata);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке файла");
            return StatusCode(StatusCodes.Status500InternalServerError, "Внутренняя ошибка сервера при загрузке файла");
        }
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetFile(Guid id)
    {
        try
        {
            var (fileContent, contentType) = await _fileStorageService.GetFileAsync(id);
            return File(fileContent, contentType);
        }
        catch (FileNotFoundException)
        {
            return NotFound($"Файл с ID {id} не найден");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении файла с ID {FileId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Внутренняя ошибка сервера при получении файла");
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<FileMetadata>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllFiles()
    {
        try
        {
            var files = await _fileStorageService.GetAllFilesMetadataAsync();
            return Ok(files);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении списка файлов");
            return StatusCode(StatusCodes.Status500InternalServerError, "Внутренняя ошибка сервера при получении списка файлов");
        }
    }

    [HttpGet("metadata/{id}")]
    [ProducesResponseType(typeof(FileMetadata), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetFileMetadata(Guid id)
    {
        try
        {
            var metadata = await _fileStorageService.GetFileMetadataAsync(id);
            return Ok(metadata);
        }
        catch (FileNotFoundException)
        {
            return NotFound($"Метаданные для файла с ID {id} не найдены");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении метаданных файла с ID {FileId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Внутренняя ошибка сервера при получении метаданных файла");
        }
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteFile(Guid id)
    {
        try
        {
            var isDeleted = await _fileStorageService.DeleteFileAsync(id);
            if (isDeleted)
            {
                return Ok($"Файл с ID {id} успешно удален");
            }
            return NotFound($"Файл с ID {id} не найден");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении файла с ID {FileId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Внутренняя ошибка сервера при удалении файла");
        }
    }
}