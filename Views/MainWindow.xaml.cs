using System.Windows;
using EmailAttachmentExtractor.ViewModels;

namespace EmailAttachmentExtractor.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
}