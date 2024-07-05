using EmailAttachmentExtractor.Commands.Base;
using EmailAttachmentExtractor.ViewModels;

namespace EmailAttachmentExtractor.Commands;

/// <summary>
///     Команда для выбора директории с электронными письмами.
/// </summary>
public class SelectEmailFolderCommand(MainViewModel vm) : Command
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
    ///     Выполняет команду по выбору директории с электронными письмами.
    /// </summary>
    /// <param name="parameter">Параметр команды.</param>
    public override void Execute(object? parameter)
    {
        vm.SelectEmailDirectory();
    }
}