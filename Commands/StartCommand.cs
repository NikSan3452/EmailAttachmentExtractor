using System.Windows.Forms;
using EmailAttachmentExtractor.Commands.Base;
using EmailAttachmentExtractor.ViewModels;

namespace EmailAttachmentExtractor.Commands;

public class StartCommand(MainViewModel vm) : Command
{
    public override bool CanExecute(object? parameter)
    {
        return true;
    }

    public override async void Execute(object? parameter)
    {
        if (!string.IsNullOrEmpty(vm.EmailPath) && !string.IsNullOrEmpty(vm.AttachmentsDirectory))
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