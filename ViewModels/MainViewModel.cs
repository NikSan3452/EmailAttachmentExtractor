using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using EmailAttachmentExtractor.Commands;
using EmailAttachmentExtractor.Services;
using EmailAttachmentExtractor.ViewModels.Base;

namespace EmailAttachmentExtractor.ViewModels;

[MarkupExtensionReturnType(typeof(MainViewModel))]
public class MainViewModel : ViewModel
{
    private readonly EmailAttachmentExtractService _extractService;

    public MainViewModel()
    {
        #region Commands

        _extractService = new EmailAttachmentExtractService();
        StartCommand = new StartCommand(this);
        SelectEmailFolderCommand = new SelectEmailFolderCommand(this);
        SelectAttachmentsFolderCommand = new SelectAttachmentsFolderCommand(this);

        #endregion
    }

    public void SelectEmailPath()
    {
        EmailPath = _extractService.SelectEmailPath();
    }

    public void SelectAttachmentsFolder()
    {
        AttachmentsDirectory = _extractService.SelectAttachmentsFolder();
    }

    public void ExtractAttachments()
    {
        if (!string.IsNullOrEmpty(EmailPath) && !string.IsNullOrEmpty(AttachmentsDirectory))
        {
            _extractService.ExtractAttachments(EmailPath, AttachmentsDirectory);
            MessageBox.Show("Вложения успешно извлечены.");
        }
        else
        {
            MessageBox.Show("Пожалуйста, выберите путь к файлу/папке .eml и папку для сохранения вложений.");
        }
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

    #endregion
}