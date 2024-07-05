using System.Windows.Input;

namespace EmailAttachmentExtractor.Commands.Base;

/// <summary>
///     Абстрактный базовый класс для всех команд, реализующий интерфейс ICommand.
/// </summary>
public abstract class Command : ICommand
{
    /// <summary>
    ///     Событие, возникающее при изменении условий, которые могут повлиять на возможность выполнения команды.
    /// </summary>
    /// <remarks>
    ///     Это событие автоматически подписывается на событие CommandManager.RequerySuggested,
    ///     которое вызывается, когда система считает, что условия выполнения команд могли измениться.
    /// </remarks>
    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    /// <summary>
    ///     Определяет, может ли команда выполняться.
    /// </summary>
    /// <param name="parameter">Параметр команды.</param>
    /// <returns>True, если команда может быть выполнена; в противном случае, false.</returns>
    public abstract bool CanExecute(object? parameter);

    /// <summary>
    ///     Выполняет логику команды.
    /// </summary>
    /// <param name="parameter">Параметр команды.</param>
    public abstract void Execute(object? parameter);
}