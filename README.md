# Система анализа плагиата (Anti-Plagiat)

##  Основные возможности

###  Управление файлами

- Загрузка, просмотр, скачивание и удаление файлов через контроллер `FilesController`
- Хранение метаданных файлов в базе данных (`FileMetadata`)
- Физическое сохранение файлов на диске с помощью `FileStorageService`

###  Проверка на плагиат

- Анализ загруженных файлов через `AnalysisController`
- Сохранение результатов проверки в базе данных (`FileAnalysisResult`)
- Отслеживание найденных совпадений между файлами (`SimilarityMatch`)
- Подсчёт статистики по проверенным документам

###  API Gateway

- Маршрутизация запросов между микросервисами
- Единая точка входа в систему

##  Архитектура микросервисов

### Независимые сервисы:

- **FileStoringService** — отвечает за загрузку, хранение и управление файлами  
- **FileAnalisysService** — выполняет анализ и выявляет плагиат  
- **APIGateway** — маршрутизирует запросы между сервисами

###  Разделение данных:

- **PostgreSQL базы данных:**
  - `postgres-filestoring` — метаданные файлов (**FileStoringService**)
  - `postgres-fileanalysis` — результаты анализа (**FileAnalisysService**)
-  Повышенная отказоустойчивость: сбой одной базы не влияет на работу других сервисов  
-  Взаимодействие между сервисами только через API — **никакого прямого доступа к данным**

##  Структура проекта

###  FileStoringService

- `Controllers/FilesController.cs` — API для управления файлами  
- `Data/AppDbContext.cs` — контекст базы данных для метаданных  
- `Models/FileMetadata.cs` — модель метаданных файла  
- `Services/FileStorageService.cs` — физическое хранение файлов  

###  FileAnalisysService

- `Controllers/AnalysisController.cs` — API для анализа  
- `Data/AnalysisDbContext.cs` — контекст базы данных анализа  
- `Models/FileAnalysisResult.cs`, `SimilarityMatch.cs` — модели результатов анализа и совпадений  
- `Services/FileAnalysisService.cs` — логика проверки файлов  

###  APIGateway

- `Program.cs` — настройка маршрутов и прокси

##  Особенности работы

- При загрузке нового файла запускается анализ:
  -  Сравнение идёт со всеми ранее загруженными файлами
  -  Совпадающие имена = 100% плагиат
  -  Разные имена = случайный процент совпадения от `0%` до `60%`
  -  При совпадении от `30%` и выше — результат фиксируется как плагиат

##  Запуск и тестирование

### Запуск проекта:

```bash
docker compose up --build
```

_Запускать из корня проекта_

###  Доступ к сервисам:

| Сервис                  | URL                                                                 |
|------------------------|----------------------------------------------------------------------|
| **API Gateway**        | [`http://localhost:7001/api/*`](http://localhost:7001/api/)          |
| — File API             | `/api/files/*` → перенаправляется в FileStoringService              |
| — Analysis API         | `/api/analysis/*` → перенаправляется в FileAnalisysService          |
| **FileStoring Swagger**| [`http://localhost:7002/swagger/index.html`](http://localhost:7002/swagger/index.html) |
| **Analysis Swagger**   | [`http://localhost:7003/swagger/index.html`](http://localhost:7003/swagger/index.html) |
