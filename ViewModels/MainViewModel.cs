using System.Diagnostics;
using System.Timers;
using System.Windows.Input;
using System.Windows.Markup;
using EmailAttachmentExtractor.Commands;
using EmailAttachmentExtractor.Helpers;
using EmailAttachmentExtractor.Helpers.Interfaces;
using EmailAttachmentExtractor.Services;
using EmailAttachmentExtractor.ViewModels.Base;
using Ookii.Dialogs.Wpf;
using Timer = System.Timers.Timer;

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
    private readonly EmailAttachmentExtractService _extractService;

    /// <summary>
    ///     Таймер для отслеживания времени выполнения процесса.
    /// </summary>
    private Stopwatch? _stopwatch;

    /// <summary>
    ///     Таймер для обновления времени выполнения каждую секунду.
    /// </summary>
    private Timer? _timer;

    public MainViewModel()
    {
        ITextEncoder textEncoder = new TextEncoder();
        _extractService = new EmailAttachmentExtractService(textEncoder);
        _extractService.ProgressChanged += OnProgressChanged;

        ProgressValue = 0;
        ProcessedFilesCount = 0;

        #region Commands

        StartCommand = new StartCommand(this);
        SelectEmailFolderCommand = new SelectEmailFolderCommand(this);
        SelectAttachmentsFolderCommand = new SelectAttachmentsFolderCommand(this);

        #endregion
    }

    /// <summary>
    ///     Запускает асинхронный процесс извлечения вложений.
    /// </summary>
    public async Task StartExtractionAsync()
    {
        StartTimer();
        await _extractService.ExtractAttachmentsAsync();
        StopTimer();
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

    #region EmailDirectory

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
            _extractService.EmailDirectory = value;
        }
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

    #endregion

    #region AttachmentsDirectory

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
            _extractService.AttachmentsDirectory = value;
        }
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

    #endregion

    #region Progress
    
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

    #endregion

    #region ExecutionTime

    private string? _executionTime;

    /// <summary>
    ///     Время выполнения процесса.
    /// </summary>
    public string? ExecutionTime
    {
        get => _executionTime;
        private set => Set(ref _executionTime, value);
    }

    /// <summary>
    ///     Запускает таймер для отслеживания времени выполнения процесса.
    /// </summary>
    private void StartTimer()
    {
        _stopwatch = Stopwatch.StartNew();
        _timer = new Timer(1000);
        _timer.Elapsed += UpdateExecutionTime;
        _timer.Start();
    }

    /// <summary>
    ///     Останавливает таймер и обновляет время выполнения процесса.
    /// </summary>
    private void StopTimer()
    {
        _stopwatch?.Stop();
        _timer?.Stop();
        UpdateExecutionTime(this, null);
    }

    /// <summary>
    ///     Обновляет время выполнения процесса.
    /// </summary>
    /// <param name="sender">Источник события.</param>
    /// <param name="e">Аргументы события.</param>
    private void UpdateExecutionTime(object? sender, ElapsedEventArgs? e)
    {
        ExecutionTime = _stopwatch?.Elapsed.ToString(@"hh\:mm\:ss");
    }

    #endregion
}