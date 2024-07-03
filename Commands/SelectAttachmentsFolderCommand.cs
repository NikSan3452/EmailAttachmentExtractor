using EmailAttachmentExtractor.Commands.Base;
using EmailAttachmentExtractor.ViewModels;

namespace EmailAttachmentExtractor.Commands;

public class SelectAttachmentsFolderCommand(MainViewModel vm) : Command
{
    public override bool CanExecute(object? parameter)
    {
        return true;
    }

    public override void Execute(object? parameter)
    {
        vm.SelectAttachmentsFolder();
    }
}