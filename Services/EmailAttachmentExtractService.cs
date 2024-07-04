using System.IO;
using MimeKit;
using Ookii.Dialogs.Wpf;

namespace EmailAttachmentExtractor.Services;

public class EmailAttachmentExtractService
{
    public string? EmailPath { get; private set; }
    public string? AttachmentsDirectory { get; private set; }

    public event Action<int, int>? ProgressChanged;

    public string? SelectEmailPath()
    {
        var folderDialog = new VistaFolderBrowserDialog
        {
            Description = "Выберите папку, содержащую файлы .eml",
            UseDescriptionForTitle = true
        };

        EmailPath = folderDialog.ShowDialog() == true ? folderDialog.SelectedPath : null;
        return EmailPath;
    }

    public string? SelectAttachmentsFolder()
    {
        var dialog = new VistaFolderBrowserDialog
        {
            Description = "Выберите папку для сохранения вложений",
            UseDescriptionForTitle = true
        };

        AttachmentsDirectory = dialog.ShowDialog() == true ? dialog.SelectedPath : null;
        return AttachmentsDirectory;
    }

    public async Task ExtractAttachmentsAsync()
    {
        if (EmailPath == null || AttachmentsDirectory == null)
            return;

        if (Directory.Exists(EmailPath))
        {
            var emailFiles = GetEmailFiles(EmailPath).ToList();
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

    private IEnumerable<string> GetEmailFiles(string directory)
    {
        var emailFiles = new List<string>();

        try
        {
            emailFiles.AddRange(Directory.GetFiles(directory, "*.eml"));

            foreach (var subDirectory in Directory.GetDirectories(directory))
                emailFiles.AddRange(GetEmailFiles(subDirectory));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при обработке директории {directory}: {ex.Message}");
        }

        return emailFiles;
    }

    private async Task ProcessEmailFileAsync(string emailFilePath, string? attachmentDirectory)
    {
        try
        {
            var message = await LoadEmailMessageAsync(emailFilePath);
            var subject = SanitizeFileName(message.Subject);
            var uniqueFolderName = $"{subject}_{Guid.NewGuid()}";

            if (attachmentDirectory == null) return;

            var messageDirectory = Path.Combine(attachmentDirectory, uniqueFolderName);
            Directory.CreateDirectory(messageDirectory);

            await SaveEmailBodyAsync(message, messageDirectory);

            if (message.Attachments.Any()) await SaveAttachmentsAsync(message, messageDirectory);

            SaveEmbeddedImages(message.Body, messageDirectory);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при обработке файла {emailFilePath}: {ex.Message}");
        }
    }

    private async Task<MimeMessage> LoadEmailMessageAsync(string emailFilePath)
    {
        return await Task.Run(() => MimeMessage.Load(emailFilePath));
    }

    private string SanitizeFileName(string fileName)
    {
        return string.Join("_", fileName.Split(Path.GetInvalidFileNameChars()));
    }

    private async Task SaveEmailBodyAsync(MimeMessage message, string messageDirectory)
    {
        var htmlFilePath = Path.Combine(messageDirectory, "email.html");
        await File.WriteAllTextAsync(htmlFilePath, message.HtmlBody ?? message.TextBody);
    }

    private async Task SaveAttachmentsAsync(MimeMessage message, string messageDirectory)
    {
        foreach (var attachment in message.Attachments.OfType<MimePart>())
        {
            var fileName = attachment.FileName;

            if (string.IsNullOrWhiteSpace(fileName)) fileName = Guid.NewGuid().ToString();

            var filePath = Path.Combine(messageDirectory, fileName);

            await using (var stream = File.Create(filePath))
            {
                await attachment.Content.DecodeToAsync(stream);
            }

            Console.WriteLine($"Сохранено вложение: {fileName} в папку {messageDirectory}");
        }
    }

    private void SaveEmbeddedImages(MimeEntity entity, string messageDirectory)
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

                    using (var stream = File.Create(filePath))
                    {
                        part.Content.DecodeTo(stream);
                    }

                    Console.WriteLine($"Сохранено изображение: {fileName} в папку {messageDirectory}");
                }

                break;
        }
    }
}