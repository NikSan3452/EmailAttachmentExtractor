using System.Windows.Input;
using System.Windows.Markup;
using EmailAttachmentExtractor.Commands;
using EmailAttachmentExtractor.Services;
using EmailAttachmentExtractor.ViewModels.Base;

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

    #region Commands

    public ICommand StartCommand { get; set; }
    public ICommand SelectEmailFolderCommand { get; set; }
    public ICommand SelectAttachmentsFolderCommand { get; set; }

    #endregion

    #region Properties

    private string? _emailPath;

    public string? EmailPath
    {
        get => _emailPath;
        set => Set(ref _emailPath, value);
    }

    private string? _attachmentsDirectory;

    public string? AttachmentsDirectory
    {
        get => _attachmentsDirectory;
        set => Set(ref _attachmentsDirectory, value);
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
        set => Set(ref _processedFilesCount, value);
    }

    #endregion
}