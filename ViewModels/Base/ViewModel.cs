using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Markup;
using System.Windows.Threading;
using System.Xaml;

namespace EmailAttachmentExtractor.ViewModels.Base;

/// <summary>
///     Базовый класс для всех ViewModel, реализующий INotifyPropertyChanged и MarkupExtension.
/// </summary>
public abstract class ViewModel : MarkupExtension, INotifyPropertyChanged
{
    private WeakReference? _rootRef;
    private WeakReference? _targetRef;

    /// <summary>
    ///     Объект, к которому привязана ViewModel.
    /// </summary>
    public object? TargetObject => _targetRef?.Target;

    /// <summary>
    ///     Корневой объект, к которому привязана ViewModel.
    /// </summary>
    public object? RootObject => _rootRef?.Target;

    /// <summary>
    ///     Событие, возникающее при изменении свойства.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    ///     Вызывает событие PropertyChanged для указанного свойства.
    /// </summary>
    /// <remarks>
    ///     Метод выполняет следующие действия:
    ///     <list type="number">
    ///         <item>Проверяет, есть ли подписчики на событие PropertyChanged.</item>
    ///         <item>Если подписчики есть, получает список всех подписчиков.</item>
    ///         <item>Для каждого подписчика проверяет, является ли он DispatcherObject.</item>
    ///         <item>Если подписчик является DispatcherObject, вызывает его через Dispatcher.</item>
    ///         <item>Если подписчик не является DispatcherObject, вызывает его напрямую.</item>
    ///     </list>
    /// </remarks>
    /// <param name="propertyName">Имя свойства, которое изменилось.</param>
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        var handlers = PropertyChanged;
        if (handlers == null) return;
        var invocationList = handlers.GetInvocationList();
        var arg = new PropertyChangedEventArgs(propertyName);

        foreach (var action in invocationList)
            if (action.Target is DispatcherObject dispObject)
                dispObject.Dispatcher.Invoke(action, this, arg);
            else
                action.DynamicInvoke(this, arg);
    }

    /// <summary>
    ///     Устанавливает значение поля и вызывает событие PropertyChanged, если значение изменилось.
    /// </summary>
    /// <remarks>
    ///     Метод выполняет следующие действия:
    ///     <list type="number">
    ///         <item>Сравнивает текущее значение поля с новым значением.</item>
    ///         <item>Если значения различаются, устанавливает новое значение поля.</item>
    ///         <item>Вызывает метод OnPropertyChanged для уведомления об изменении свойства.</item>
    ///     </list>
    /// </remarks>
    /// <typeparam name="T">Тип поля.</typeparam>
    /// <param name="field">Ссылка на поле.</param>
    /// <param name="value">Новое значение.</param>
    /// <param name="propertyName">Имя свойства.</param>
    /// <returns>True, если значение было изменено, иначе false.</returns>
    protected virtual bool Set<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    /// <summary>
    ///     Предоставляет значение для XAML.
    /// </summary>
    /// <remarks>
    ///     Метод выполняет следующие действия:
    ///     <list type="number">
    ///         <item>Получает сервис IProvideValueTarget для определения целевого объекта и свойства.</item>
    ///         <item>Получает сервис IRootObjectProvider для определения корневого объекта.</item>
    ///         <item>Вызывает метод OnInitialized для инициализации ссылок на целевой и корневой объекты.</item>
    ///     </list>
    /// </remarks>
    /// <param name="sp">Провайдер служб.</param>
    /// <returns>Текущий экземпляр ViewModel.</returns>
    public override object ProvideValue(IServiceProvider sp)
    {
        var valueTargetService = sp.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
        var rootObjectService = sp.GetService(typeof(IRootObjectProvider)) as IRootObjectProvider;

        OnInitialized(
            valueTargetService?.TargetObject,
            valueTargetService?.TargetProperty,
            rootObjectService?.RootObject);

        return this;
    }

    /// <summary>
    ///     Вызывается при инициализации ViewModel.
    /// </summary>
    /// <remarks>
    ///     Метод выполняет следующие действия:
    ///     <list type="number">
    ///         <item>Создает слабые ссылки на целевой и корневой объекты.</item>
    ///     </list>
    /// </remarks>
    /// <param name="target">Целевой объект.</param>
    /// <param name="property">Целевое свойство.</param>
    /// <param name="root">Корневой объект.</param>
    protected virtual void OnInitialized(object? target, object? property, object? root)
    {
        _targetRef = new WeakReference(target);
        _rootRef = new WeakReference(root);
    }
}