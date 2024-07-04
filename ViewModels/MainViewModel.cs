using System.Windows.Input;
using System.Windows.Markup;
using EmailAttachmentExtractor.Commands;
using EmailAttachmentExtractor.Services;
using EmailAttachmentExtractor.ViewModels.Base;
using Ookii.Dialogs.Wpf;

namespace EmailAttachmentExtractor.ViewModels;

[MarkupExtensionReturnType(typeof(MainViewModel))]
public class MainViewModel : ViewModel
{
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

    private void OnProgressChanged(int processedFiles, int progress)
    {
        ProgressValue = progress;
        ProcessedFilesCount = processedFiles;
    }

    public void SelectEmailDirectory()
    {
        var folderDialog = new VistaFolderBrowserDialog
        {
            Description = "Выберите папку, содержащую файлы .eml",
            UseDescriptionForTitle = true
        };

        EmailDirectory = folderDialog.ShowDialog() == true ? folderDialog.SelectedPath : null;
    }

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

    public ICommand StartCommand { get; set; }
    public ICommand SelectEmailFolderCommand { get; set; }
    public ICommand SelectAttachmentsFolderCommand { get; set; }

    #endregion

    #region Properties

    private string? _emailDirectory;

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

    public int ProgressValue
    {
        get => _progressValue;
        set => Set(ref _progressValue, value);
    }

    private int _processedFilesCount;

    public int ProcessedFilesCount
    {
        get => _processedFilesCount;
        private set => Set(ref _processedFilesCount, value);
    }

    #endregion
}