using System.IO;
using MimeKit;
using Ookii.Dialogs.Wpf;

namespace EmailAttachmentExtractor.Services;

public class EmailAttachmentExtractService
{
    public string? SelectEmailPath()
    {
        var folderDialog = new VistaFolderBrowserDialog
        {
            Description = "Выберите папку, содержащую файлы .eml",
            UseDescriptionForTitle = true
        };

        return folderDialog.ShowDialog() == true ? folderDialog.SelectedPath : null;
    }

    public string? SelectAttachmentsFolder()
    {
        var dialog = new VistaFolderBrowserDialog
        {
            Description = "Выберите папку для сохранения вложений",
            UseDescriptionForTitle = true
        };

        return dialog.ShowDialog() == true ? dialog.SelectedPath : null;
    }

    public void ExtractAttachments(string? emailPath, string? attachmentsDirectory)
    {
        if (File.Exists(emailPath))
        {
            ProcessEmailFile(emailPath, attachmentsDirectory);
        }
        else if (Directory.Exists(emailPath))
        {
            var emlFiles = GetEmailFiles(emailPath);

            foreach (var emlFile in emlFiles) ProcessEmailFile(emlFile, attachmentsDirectory);
        }
    }

    private IEnumerable<string?> GetEmailFiles(string? directory)
    {
        var emlFiles = new List<string?>();

        try
        {
            if (directory != null)
            {
                emlFiles.AddRange(Directory.GetFiles(directory, "*.eml"));

                foreach (var subDirectory in Directory.GetDirectories(directory))
                    emlFiles.AddRange(GetEmailFiles(subDirectory));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при обработке директории {directory}: {ex.Message}");
        }

        return emlFiles;
    }

    private void ProcessEmailFile(string? emlFilePath, string? outputDirectory)
    {
        try
        {
            var message = MimeMessage.Load(emlFilePath);
            var subject = string.Join("_", message.Subject.Split(Path.GetInvalidFileNameChars()));

            if (outputDirectory == null) return;

            var messageDirectory = Path.Combine(outputDirectory, subject);
            Directory.CreateDirectory(messageDirectory);

            foreach (var attachment in message.Attachments)
                if (attachment is MimePart part)
                {
                    var fileName = part.FileName;
                    var filePath = Path.Combine(messageDirectory, fileName);

                    using (var stream = File.Create(filePath))
                    {
                        part.Content.DecodeTo(stream);
                    }

                    Console.WriteLine($"Сохранено вложение: {fileName} в папку {messageDirectory}");
                }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при обработке файла {emlFilePath}: {ex.Message}");
        }
    }
}