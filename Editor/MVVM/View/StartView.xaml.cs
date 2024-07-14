using System.Windows;
using System.Windows.Controls;
using Editor.MVVM.ViewModel;
using Microsoft.Win32;

namespace Editor.MVVM.View
{
    public partial class StartView : UserControl
    {
        public StartView()
        {
            InitializeComponent();
        }


        private void OpenFileClick(object sender, RoutedEventArgs e)
        {
            var viewModel = (StartViewModel)DataContext;
            var dialog = new OpenFileDialog
            {
                Multiselect = false,
                Title = "Select btf file",
                DefaultExt = ".btf",
                Filter = "Byte Text Files (*.btf)|*.btf"
            };
            var result = dialog.ShowDialog();
            if(!result.HasValue || !result.Value) return;

            
            if(viewModel.LoadFileCommand.CanExecute(dialog.FileName))
                viewModel.LoadFileCommand.Execute(dialog.FileName);
        }
    }
}