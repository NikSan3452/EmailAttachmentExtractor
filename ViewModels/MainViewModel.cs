using System.Windows.Input;
using System.Windows.Markup;
using EmailAttachmentExtractor.Commands;
using EmailAttachmentExtractor.Services;
using EmailAttachmentExtractor.ViewModels.Base;
using Ookii.Dialogs.Wpf;

namespace EmailAttachmentExtractor.ViewModels;

/// <summary>
///     ViewModel для приложения извлечения вложений из электронных писем.
/// </summary>
[MarkupExtensionReturnType(typeof(MainViewModel))]
public class MainViewModel : ViewModel
{
    /// <summary>
    ///     Сервис для извлечения вложений из электронных писем.
    /// </summary>
    public readonly EmailAttachmentExtractService ExtractService;

    public MainViewModel()
    {
        ExtractService = new EmailAttachmentExtractService();
        ExtractService.ProgressChanged += OnProgressChanged;
        ProgressValue = 0;
        ProcessedFilesCount = 0;

        #region Commands

        StartCommand = new StartCommand(this);
        SelectEmailFolderCommand = new SelectEmailFolderCommand(this);
        SelectAttachmentsFolderCommand = new SelectAttachmentsFolderCommand(this);

        #endregion
    }

    /// <summary>
    ///     Обрабатывает событие изменения прогресса извлечения вложений.
    /// </summary>
    /// <param name="processedFiles">Количество обработанных файлов.</param>
    /// <param name="progress">Текущий прогресс в процентах.</param>
    private void OnProgressChanged(int processedFiles, int progress)
    {
        ProgressValue = progress;
        ProcessedFilesCount = processedFiles;
    }

    /// <summary>
    ///     Открывает диалог выбора директории с электронными письмами.
    /// </summary>
    public void SelectEmailDirectory()
    {
        var folderDialog = new VistaFolderBrowserDialog
        {
            Description = "Выберите папку, содержащую файлы .eml",
            UseDescriptionForTitle = true
        };

        EmailDirectory = folderDialog.ShowDialog() == true ? folderDialog.SelectedPath : null;
    }

    /// <summary>
    ///     Открывает диалог выбора директории для сохранения вложений.
    /// </summary>
    public void SelectAttachmentsDirectory()
    {
        var dialog = new VistaFolderBrowserDialog
        {
            Description = "Выберите папку для сохранения вложений",
            UseDescriptionForTitle = true
        };

        AttachmentsDirectory = dialog.ShowDialog() == true ? dialog.SelectedPath : null;
    }

    #region Commands

    /// <summary>
    ///     Команда для начала процесса извлечения вложений.
    /// </summary>
    public ICommand StartCommand { get; set; }

    /// <summary>
    ///     Команда для выбора директории с электронными письмами.
    /// </summary>
    public ICommand SelectEmailFolderCommand { get; set; }

    /// <summary>
    ///     Команда для выбора директории для сохранения вложений.
    /// </summary>
    public ICommand SelectAttachmentsFolderCommand { get; set; }

    #endregion

    #region Properties

    private string? _emailDirectory;

    /// <summary>
    ///     Директория, содержащая электронные письма.
    /// </summary>
    public string? EmailDirectory
    {
        get => _emailDirectory;
        private set
        {
            Set(ref _emailDirectory, value);
            ExtractService.EmailDirectory = value;
        }
    }

    private string? _attachmentsDirectory;

    /// <summary>
    ///     Директория для сохранения вложений.
    /// </summary>
    public string? AttachmentsDirectory
    {
        get => _attachmentsDirectory;
        private set
        {
            Set(ref _attachmentsDirectory, value);
            ExtractService.AttachmentsDirectory = value;
        }
    }

    private int _progressValue;

    /// <summary>
    ///     Текущий прогресс процесса извлечения вложений в процентах.
    /// </summary>
    public int ProgressValue
    {
        get => _progressValue;
        set => Set(ref _progressValue, value);
    }

    private int _processedFilesCount;

    /// <summary>
    ///     Количество обработанных файлов электронных писем.
    /// </summary>
    public int ProcessedFilesCount
    {
        get => _processedFilesCount;
        private set => Set(ref _processedFilesCount, value);
    }

    #endregion
}