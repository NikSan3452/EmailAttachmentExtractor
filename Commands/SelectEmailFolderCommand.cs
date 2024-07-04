using EmailAttachmentExtractor.Commands.Base;
using EmailAttachmentExtractor.ViewModels;

namespace EmailAttachmentExtractor.Commands;

public class SelectEmailFolderCommand(MainViewModel vm) : Command
{
    public override bool CanExecute(object? parameter)
    {
        return true;
    }

    public override void Execute(object? parameter)
    {
        vm.SelectEmailDirectory();
    }
}