Система анализа плагиата (Anti-Plagiat)
Основные возможности
Управление файлами

Загрузка, просмотр, скачивание и удаление файлов через контроллер FilesController

Хранение метаданных файлов в базе данных (FileMetadata)

Физическое сохранение файлов на диске с помощью сервиса FileStorageService

Проверка на плагиат

Анализ загруженных файлов через контроллер AnalysisController

Сохранение результатов проверки в базе данных (FileAnalysisResult)

Отслеживание найденных совпадений между файлами (SimilarityMatch)

Подсчёт статистики по проверенным документам

API Gateway

Обеспечивает маршрутизацию запросов между микросервисами

Выполняет роль единой точки входа в систему

Архитектура микросервисов
Отдельные независимые сервисы:

FileStoringService — отвечает за загрузку, хранение и управление файлами

FileAnalisysService — выполняет анализ и выявляет плагиат среди сохранённых файлов

APIGateway — маршрутизирует запросы между сервисами

Разделение данных:

Каждый сервис использует свою базу данных:

postgres-filestoring — для метаданных файлов FileStoringService

postgres-fileanalysis — для результатов анализа FileAnalisysService

Это повышает отказоустойчивость: сбой одной базы не блокирует работу другого сервиса

Сервисы обмениваются данными только через API, прямого доступа к чужим данным нет

Структура проекта
FileStoringService

API для работы с файлами (Controllers/FilesController.cs)

Контекст базы данных метаданных (Data/AppDbContext.cs)

Модель метаданных файла (Models/FileMetadata.cs)

Сервис хранения файлов на диске (Services/FileStorageService.cs)

FileAnalisysService

API для проверки на плагиат (Controllers/AnalysisController.cs)

Контекст базы данных результатов анализа (Data/AnalysisDbContext.cs)

Модели результата анализа и совпадений (Models/FileAnalysisResult.cs, Models/SimilarityMatch.cs)

Логика анализа файлов (Services/FileAnalysisService.cs)

APIGateway

Основной файл настройки маршрутизации (Program.cs)

Особенности работы
При загрузке файла проводится сравнение с другими файлами

Если имена файлов совпадают — плагиат считается 100%

Если имена разные — генерируется случайный процент совпадения от 0 до 60

При совпадении 30% и выше, в результатах указывается ссылка на файл с плагиатом

Запуск и тестирование
Для запуска используйте команду:
docker compose up --build
из корня проекта

Проверить работу сервисов можно по адресам:

API Gateway: http://localhost:7001/api/*
(например, /api/files/* для FileStoringService, /api/analysis/* для FileAnalisysService)

FileStoringService Swagger UI: http://localhost:7002/swagger/index.html

FileAnalisysService Swagger UI: http://localhost:7003/swagger/index.html