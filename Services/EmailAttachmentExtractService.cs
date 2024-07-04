using System.IO;
using System.Windows;
using MimeKit;

namespace EmailAttachmentExtractor.Services;

public class EmailAttachmentExtractService
{
    public string? EmailDirectory { get; set; }
    public string? AttachmentsDirectory { get; set; }

    public event Action<int, int>? ProgressChanged;

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

    private static async Task ProcessEmailFileAsync(string emailFileDirectory, string? attachmentDirectory)
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

    private static async Task<MimeMessage> LoadEmailMessageAsync(string emailFileDirectory)
    {
        return await Task.Run(() => MimeMessage.Load(emailFileDirectory));
    }

    private static string SanitizeFileName(string fileName)
    {
        return string.Join("_", fileName.Split(Path.GetInvalidFileNameChars()));
    }

    private static async Task SaveEmailBodyAsync(MimeMessage message, string messageDirectory)
    {
        var htmlFilePath = Path.Combine(messageDirectory, "email.html");
        await File.WriteAllTextAsync(htmlFilePath, message.HtmlBody ?? message.TextBody);
    }

    private static async Task SaveAttachmentsAsync(MimeMessage message, string messageDirectory)
    {
        foreach (var attachment in message.Attachments.OfType<MimePart>())
        {
            var fileName = attachment.FileName;

            if (string.IsNullOrWhiteSpace(fileName)) fileName = Guid.NewGuid().ToString();

            var filePath = Path.Combine(messageDirectory, fileName);

            await using var stream = File.Create(filePath);
            await attachment.Content.DecodeToAsync(stream);
        }
    }

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