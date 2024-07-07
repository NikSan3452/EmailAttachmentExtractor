using System.IO;
using System.Text;
using System.Windows;
using EmailAttachmentExtractor.Helpers.Interfaces;
using MimeKit;

namespace EmailAttachmentExtractor.Services;

/// <summary>
///     Сервис для извлечения вложений из электронной почты.
/// </summary>
public class EmailAttachmentExtractService(ITextEncoder encoder)
{
    /// <summary>
    ///     Директория с email-файлами.
    /// </summary>
    public string? EmailDirectory { get; set; }

    /// <summary>
    ///     Директория для сохранения вложений.
    /// </summary>
    public string? AttachmentsDirectory { get; set; }

    /// <summary>
    ///     Событие, вызываемое при изменении прогресса обработки email-файлов.
    /// </summary>
    public event Action<int, int>? ProgressChanged;

    /// <summary>
    ///     Асинхронно извлекает вложения из электронных писем в указанной директории.
    /// </summary>
    /// <remarks>
    ///     Метод выполняет следующие действия:
    ///     <list type="number">
    ///         <item>Проверяет существование директории с электронными письмами.</item>
    ///         <item>Получает список файлов электронных писем.</item>
    ///         <item>Обрабатывает каждый файл, извлекая вложения.</item>
    ///         <item>Отслеживает прогресс обработки и вызывает событие ProgressChanged.</item>
    ///     </list>
    /// </remarks>
    /// <seealso cref="GetEmailFiles" />
    /// <seealso cref="ProcessEmailFileAsync" />
    public async Task ExtractAttachmentsAsync()
    {
        if (Directory.Exists(EmailDirectory))
        {
            var emailFiles = GetEmailFiles(EmailDirectory).ToList();
            var fileCount = emailFiles.Count;
            var processedFiles = 0;

            foreach (var emailFile in emailFiles)
            {
                await ProcessEmailFileAsync(emailFile, AttachmentsDirectory);
                processedFiles++;
                var progress = processedFiles * 100 / fileCount;
                ProgressChanged?.Invoke(processedFiles, progress);
            }
        }
    }

    /// <summary>
    ///     Рекурсивно получает список файлов электронных писем из указанной директории и ее поддиректорий.
    /// </summary>
    /// <param name="directory">Путь к директории для поиска файлов электронных писем.</param>
    /// <returns>Список путей к файлам электронных писем с расширением .eml.</returns>
    /// <remarks>
    ///     Метод выполняет следующие действия:
    ///     <list type="number">
    ///         <item>Создает список для хранения путей к файлам.</item>
    ///         <item>Добавляет все файлы с расширением .eml из указанной директории.</item>
    ///         <item>Рекурсивно обрабатывает все поддиректории.</item>
    ///         <item>Обрабатывает возможные исключения и отображает сообщение об ошибке.</item>
    ///     </list>
    ///     В случае возникновения любых исключений, метод отображает сообщение об ошибке.
    /// </remarks>
    private static List<string> GetEmailFiles(string? directory)
    {
        var emailFiles = new List<string>();

        try
        {
            if (directory != null)
            {
                emailFiles.AddRange(Directory.GetFiles(directory, "*.eml"));

                foreach (var subDirectory in Directory.GetDirectories(directory))
                    emailFiles.AddRange(GetEmailFiles(subDirectory));
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при обработке директории {directory}: {ex.Message}");
        }

        return emailFiles;
    }

    /// <summary>
    ///     Асинхронно обрабатывает файл электронного письма, извлекая его содержимое и вложения.
    /// </summary>
    /// <param name="emailFileDirectory">Путь к файлу электронного письма.</param>
    /// <param name="attachmentDirectory">Директория для сохранения извлеченных данных и вложений.</param>
    /// <returns>Task, представляющий асинхронную операцию.</returns>
    /// <remarks>
    ///     Метод выполняет следующие действия:
    ///     <list type="number">
    ///         <item>Загружает сообщение из файла.</item>
    ///         <item>Создает уникальную поддиректорию для данного сообщения.</item>
    ///         <item>Сохраняет тело письма.</item>
    ///         <item>Сохраняет вложения, если они есть.</item>
    ///         <item>Извлекает и сохраняет встроенные изображения.</item>
    ///     </list>
    ///     В случае возникновения любых исключений, метод отображает сообщение об ошибке.
    /// </remarks>
    /// <seealso cref="LoadEmailMessageAsync" />
    /// <seealso cref="SaveEmailBodyAsync" />
    /// <seealso cref="SaveAttachmentsAsync" />
    /// <seealso cref="SaveEmbeddedImages" />
    private async Task ProcessEmailFileAsync(string emailFileDirectory, string? attachmentDirectory)
    {
        try
        {
            var message = await LoadEmailMessageAsync(emailFileDirectory);
            var subject = SanitizeFileName(message.Subject);
            var uniqueFolderName = $"{subject}_{Guid.NewGuid()}";

            if (attachmentDirectory != null)
            {
                var messageDirectory = Path.Combine(attachmentDirectory, uniqueFolderName);
                Directory.CreateDirectory(messageDirectory);

                await SaveEmailBodyAsync(message, messageDirectory);

                if (message.Attachments.Any()) await SaveAttachmentsAsync(message, messageDirectory);

                SaveEmbeddedImages(message.Body, messageDirectory);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при обработке файла {emailFileDirectory}: {ex.Message}");
        }
    }

    /// <summary>
    ///     Асинхронно загружает сообщение электронной почты из файла.
    /// </summary>
    /// <param name="emailFileDirectory">Путь к файлу электронного письма.</param>
    /// <returns>Task, представляющий загруженное сообщение типа MimeMessage.</returns>
    private static async Task<MimeMessage> LoadEmailMessageAsync(string emailFileDirectory)
    {
        return await Task.Run(() => MimeMessage.Load(emailFileDirectory));
    }

    /// <summary>
    ///     Очищает строку от недопустимых символов для использования в имени файла.
    /// </summary>
    /// <param name="fileName">Исходная строка, представляющая имя файла.</param>
    /// <returns>Очищенная строка, безопасная для использования в качестве имени файла.</returns>
    /// <remarks>
    ///     Метод получает имя файла в правильной кодировке,
    ///     заменяет все недопустимые символы в имени на символ подчеркивания "_",
    ///     если имя пустое, то присваивается имя "Без темы".
    /// </remarks>
    private string SanitizeFileName(string fileName)
    {
        try
        {
            var sanitizedFileName = encoder.Encode(fileName);
            if (string.IsNullOrEmpty(sanitizedFileName))
            {
                sanitizedFileName = "Без темы";
            }
            sanitizedFileName = string.Join("_", sanitizedFileName.Split(Path.GetInvalidFileNameChars()));
            return sanitizedFileName;
        }
        catch
        {
            return Guid.NewGuid().ToString();
        }
    }

    /// <summary>
    ///     Асинхронно сохраняет тело электронного письма в HTML-файл.
    /// </summary>
    /// <param name="message">Объект MimeMessage, представляющий электронное письмо.</param>
    /// <param name="messageDirectory">Директория для сохранения файла.</param>
    /// <returns>Task, представляющий асинхронную операцию сохранения.</returns>
    /// <remarks>
    ///     Если HTML-тело отсутствует, сохраняется текстовое тело письма.
    /// </remarks>
    private async Task SaveEmailBodyAsync(MimeMessage message, string messageDirectory)
    {
        var bodyText = message.HtmlBody ?? message.TextBody;

        if (bodyText != null)
        {
            var decodedBodyText = encoder.Encode(bodyText);

            var htmlFilePath = Path.Combine(messageDirectory, "email.html");
            await File.WriteAllTextAsync(htmlFilePath, decodedBodyText, Encoding.UTF8);
        }
    }

    /// <summary>
    ///     Асинхронно сохраняет вложения электронного письма.
    /// </summary>
    /// <param name="message">Объект MimeMessage, представляющий электронное письмо.</param>
    /// <param name="messageDirectory">Директория для сохранения вложений.</param>
    /// <returns>Task, представляющий асинхронную операцию сохранения вложений.</returns>
    /// <remarks>
    ///     Если имя вложения отсутствует, генерируется уникальное имя файла.
    /// </remarks>
    private static async Task SaveAttachmentsAsync(MimeMessage message, string messageDirectory)
    {
        foreach (var attachment in message.Attachments.OfType<MimePart>())
        {
            var fileName = attachment.FileName;

            if (string.IsNullOrWhiteSpace(fileName)) fileName = "Без темы" + Guid.NewGuid();

            var filePath = Path.Combine(messageDirectory, fileName);

            await using var stream = File.Create(filePath);
            await attachment.Content.DecodeToAsync(stream);
        }
    }

    /// <summary>
    ///     Рекурсивно сохраняет встроенные изображения из электронного письма.
    /// </summary>
    /// <param name="entity">Объект MimeEntity, представляющий часть электронного письма.</param>
    /// <param name="messageDirectory">Директория для сохранения изображений.</param>
    /// <remarks>
    ///     Метод выполняет следующие действия:
    ///     <list type="number">
    ///         <item>Рекурсивно обходит все части сообщения.</item>
    ///         <item>Идентифицирует встроенные изображения и вложения.</item>
    ///         <item>Сохраняет найденные изображения в указанную директорию.</item>
    ///         <item>Генерирует уникальные имена файлов для изображений без имени.</item>
    ///     </list>
    ///     Метод обрабатывает следующие типы содержимого:
    ///     <list type="bullet">
    ///         <item>Multipart: рекурсивно обрабатывает каждую часть.</item>
    ///         <item>MimePart: сохраняет, если это встроенное изображение или вложение.</item>
    ///     </list>
    /// </remarks>
    private static void SaveEmbeddedImages(MimeEntity entity, string messageDirectory)
    {
        switch (entity)
        {
            case Multipart multipart:
                foreach (var part in multipart) SaveEmbeddedImages(part, messageDirectory);
                break;
            case MimePart part:
                if (part.ContentDisposition?.Disposition == ContentDisposition.Inline || part.IsAttachment)
                {
                    var fileName = part.FileName;

                    if (string.IsNullOrWhiteSpace(fileName)) fileName = Guid.NewGuid().ToString();

                    var filePath = Path.Combine(messageDirectory, fileName);

                    using var stream = File.Create(filePath);
                    part.Content.DecodeTo(stream);
                }

                break;
        }
    }
}