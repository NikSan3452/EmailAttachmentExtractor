using System.Windows.Forms;
using EmailAttachmentExtractor.Commands.Base;
using EmailAttachmentExtractor.ViewModels;

namespace EmailAttachmentExtractor.Commands;

/// <summary>
///     Команда запуска процесса извлечения вложений из электронных писем.
/// </summary>
public class StartCommand(MainViewModel vm) : Command
{
    /// <summary>
    ///     Определяет, может ли команда выполняться.
    /// </summary>
    /// <param name="parameter">Параметр команды.</param>
    /// <returns>Всегда возвращает true.</returns>
    public override bool CanExecute(object? parameter)
    {
        return true;
    }

    /// <summary>
    ///     Выполняет команду по извлечению вложений.
    /// </summary>
    /// <param name="parameter">Параметр команды.</param>
    public override async void Execute(object? parameter)
    {
        if (!string.IsNullOrEmpty(vm.EmailDirectory) && !string.IsNullOrEmpty(vm.AttachmentsDirectory))
        {
            await vm.ExtractService.ExtractAttachmentsAsync();
            MessageBox.Show("Вложения успешно извлечены.");
        }
        else
        {
            MessageBox.Show("Пожалуйста, выберите путь к папке с файлами *.eml " +
                            "и папку для сохранения вложений.");
        }
    }
}