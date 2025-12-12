# КПО. ДЗ-3

Работа выполнена Положенко Виталием Дмитриевичем, БПИ-247.

Система для приёма текстовых работ студентов, хранения файлов, анализа текста, определения простейших признаков плагиата и построения облака слов.

## User Flow

1) Студент отправляет работу на проверку, указывая:
    - `studentId`
    - `assignmentId`
    - текстовый файл `.txt`

2) Система:
    - сохраняет файл в **File Storing Service** (файл + метаданные в БД);
    - запускает анализ в **File Analysis Service** (скачивает текст по `fileId`, определяет плагиат, строит word cloud, сохраняет отчёт в БД и PNG на диск).

3) Преподаватель запрашивает отчёты по заданию (`assignmentId`) и получает JSON со всеми сдачами и флагом плагиата.

4) Для каждой работы доступно изображение облака слов (PNG).

---

## Архитектура

**API Gateway** — единственная точка входа для клиента.  
Дальше он маршрутизирует запросы во внутренние сервисы.

### Сервисы

- **ApiGateway**
    - принимает запросы клиентов;
    - загружает файл в File Storing Service;
    - инициирует анализ в File Analysis Service;
    - отдаёт клиенту отчёты и проксирует запрос PNG облака слов.

- **File Storing Service**
    - хранит оригинальные `.txt` файлы на диске;
    - хранит метаданные (id, имя, hash, location) в Postgres;
    - отдаёт файл по `id`.

- **File Analysis Service**
    - скачивает текст файла из File Storing Service;
    - считает статистики (слова/абзацы/символы);
    - определяет простейшие признаки плагиата;
    - генерирует облако слов через QuickChart;
    - сохраняет отчёт в Postgres;
    - сохраняет PNG на диск и отдаёт PNG по `workId`.

---

## Алгоритм определения плагиата

Простейший алгоритм (реализован в `FileAnalysisService.Services.AnalysisService`):

1) Получаем текст работы из File Storing Service по `fileId`.
2) Считаем `contentHash = SHA-256(text)`.
3) В БД анализа ищем самую раннюю работу с тем же:
    - `assignmentId`
    - `contentHash`
4) Если такая работа найдена, и она принадлежит другому студенту, и она была создана раньше, то:
    - `isPlagiarism = true`
    - `originalWorkId = найденная работа`

Таким образом, плагиат фиксируется как “совпадение текста с более ранней сдачей другим студентом”.

---

## Облако слов (word cloud)

Облако слов строится через QuickChart Word Cloud API:
https://quickchart.io/documentation/word-cloud-api/

PNG сохраняется на диск в volume `./FileAnalysisService/images` (в контейнере `/images`) и доступен по endpoint’у wordcloud.

---

## Запуск (Docker Compose)

### Требования
- Docker
- Docker Compose v2

### 1) Создайте `.env` в корне проекта

Пример:

```env
POSTGRES_USER=postgres
POSTGRES_PASSWORD=postgres

FILES_DB=files_db
ANALYSIS_DB=analysis_db
```

### 2) Поднимите систему

```bash
docker compose up --build
```

### Порты

* ApiGateway: [http://localhost:8080](http://localhost:8080)
* FileStoringService: [http://localhost:8081](http://localhost:8081)
* FileAnalysisService: [http://localhost:8082](http://localhost:8082)
* Postgres (files): localhost:5432
* Postgres (analysis): localhost:5433

---

## Swagger

* ApiGateway Swagger: [http://localhost:8080/swagger](http://localhost:8080/swagger)
* FileStoringService Swagger: [http://localhost:8081/swagger](http://localhost:8081/swagger)
* FileAnalysisService Swagger: [http://localhost:8082/swagger](http://localhost:8082/swagger)

---

## API (через Gateway)

### 1) Отправка работы на проверку

`POST /api/works/submit`
`Content-Type: multipart/form-data`

Форма:

* `studentId` (string)
* `assignmentId` (string)
* `file` (.txt)

Пример:

```bash
curl -F "studentId=s1" \
     -F "assignmentId=a1" \
     -F "file=@test.txt" \
     http://localhost:8080/api/works/submit
```

Ответ (пример):

```json
{
  "workId": "24977af7-d41b-4a61-ba78-794b3f9a2552",
  "studentId": "s1",
  "assignmentId": "a1",
  "submittedAt": "2025-12-12T15:27:44.5646014+00:00",
  "wordCount": 11,
  "paragraphCount": 2,
  "characterCount": 66,
  "isPlagiarism": false,
  "originalWorkId": null,
  "status": "Completed",
  "wordCloudUrl": "http://localhost:8080/api/works/24977af7-d41b-4a61-ba78-794b3f9a2552/wordcloud"
}
```

### 2) Получить отчёты по заданию

`GET /api/works/{assignmentId}/reports`

Пример:

```bash
curl http://localhost:8080/api/works/a1/reports
```

Ответ: массив отчётов по всем сдачам этого `assignmentId`.

### 3) Скачать облако слов по работе (PNG)

`GET /api/works/{workId}/wordcloud`

Пример:

```bash
curl -o wc.png http://localhost:8080/api/works/24977af7-d41b-4a61-ba78-794b3f9a2552/wordcloud
```

---

## Как проверить функционал

### 0) Проверка, что сервисы подняты

Открыть Swagger:

* Gateway: `http://localhost:8080/swagger`
* FileStoring: `http://localhost:8081/swagger`
* FileAnalysis: `http://localhost:8082/swagger`

### 1) Отправить работу (submit)

Создать тестовый файл `test.txt`, например:

```txt
Hello world
This is a test text.
```

Команда:

```bash
curl -F "studentId=s1" \
     -F "assignmentId=a1" \
     -F "file=@test.txt" \
     http://localhost:8080/api/works/submit
```

В ответе будет `workId` и `wordCloudUrl`.

### 2) Получить отчёты по assignmentId

```bash
curl http://localhost:8080/api/works/a1/reports
```

### 3) Скачать PNG облака слов по workId

Подставь `workId` из шага (1):

```bash
curl -o wc.png http://localhost:8080/api/works/<workId>/wordcloud
```
### 4) Проверка плагиата

1. Отправь первую работу от студента `s1` (как выше).
2. Отправь тот же файл от студента `s2` с тем же `assignmentId=a1`:

```bash
curl -F "studentId=s2" \
     -F "assignmentId=a1" \
     -F "file=@test.txt" \
     http://localhost:8080/api/works/submit
```

3. Проверь отчёты:

```bash
curl http://localhost:8080/api/works/a1/reports
```

У второй сдачи должно быть:

* `isPlagiarism = true`
* `originalWorkId = <workId первой сдачи>`