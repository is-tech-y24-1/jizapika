using Avalonia.Controls;
using Avalonia.Interactivity;
using GeneticAlgo.AvaloniaInterface.ViewModels;

namespace GeneticAlgo.AvaloniaInterface
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public MainWindowViewModel ViewModel => (MainWindowViewModel)DataContext!;
        
        private async void Run_OnClick(object? sender, RoutedEventArgs e)
        {
            await ViewModel.RunAsync();
        }

        private void Stop_OnClick(object? sender, RoutedEventArgs e)
        {
            ViewModel.Stop();
        }
    }
}