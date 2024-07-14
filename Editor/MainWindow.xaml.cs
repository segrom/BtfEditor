using System.Windows;
using System.Windows.Input;
using Editor.MVVM.ViewModel;

namespace Editor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void CloseButton_OnClick(object sender, RoutedEventArgs e)
        {
            var viewModel = (MainViewModel)DataContext;

            if (viewModel.CurrentView is EditorViewModel editorViewModel 
                && !editorViewModel.CheckFileSaved()) return;
            
            Application.Current.Shutdown();
        }
        
        private void MinimizeButton_OnClick(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
        
        private void Header_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }
        
        
    }
}