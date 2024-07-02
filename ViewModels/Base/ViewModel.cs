using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Markup;
using System.Windows.Threading;
using System.Xaml;

namespace EmailAttachmentExtractor.ViewModels.Base;

public abstract class ViewModel : MarkupExtension, INotifyPropertyChanged
{
    private WeakReference? _rootRef;
    private WeakReference? _targetRef;

    public object? TargetObject => _targetRef?.Target;
    public object? RootObject => _rootRef?.Target;

    public event PropertyChangedEventHandler? PropertyChanged;

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

    protected virtual bool Set<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

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

    protected virtual void OnInitialized(object? target, object? property, object? root)
    {
        _targetRef = new WeakReference(target);
        _rootRef = new WeakReference(root);
    }
}