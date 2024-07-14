using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Windows.UI.Popups;
using btfReader;
using Editor.MVVM.ViewModel;
using Microsoft.Win32;

namespace Editor.MVVM.View
{
    public partial class EditorView : UserControl
    {
        public EditorView()
        {
            InitializeComponent();
            Unloaded += Unload;
            Loaded += Load;
        }

        private void Load(object sender, RoutedEventArgs e)
        {
            var viewModel = (EditorViewModel)DataContext;
            viewModel.ItemsRefresh += ItemsRefresh;
        }
        
        private void Unload(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as EditorViewModel;
            viewModel?.Dispose();
        }

        private void ItemsRefresh()
        {
            WritingsList.Items.Refresh();
        }

        
        private void OnListScroll(object sender, MouseWheelEventArgs e)
        {
            if(sender is Windows.UI.Xaml.Controls.ScrollViewer scv)
            {
                scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta / 2f);
                e.Handled = true;
            }
        }

        private void OnItemSelected(object sender, RoutedEventArgs e)
        {
            if (WritingsList.SelectedItem is IBtfString s)
            {
                var viewModel = (EditorViewModel)DataContext;
                viewModel.SelectItem(s.Id);
            }
        }

        private void SaveFileClick(object sender, RoutedEventArgs e)
        {
            
            var viewModel = (EditorViewModel)DataContext;
            var dialog = new SaveFileDialog()
            {
                Title = "New btf file",
                DefaultExt = ".btf",
                Filter = "Byte Text Files (*.btf)|*.btf"
            };
            var result = dialog.ShowDialog();
            if(!result.HasValue || !result.Value) return;
            
            viewModel.SaveFile(dialog.FileName);
        }

        private void OpenFileClick(object sender, RoutedEventArgs e)
        {
            var viewModel = (EditorViewModel)DataContext;

            if (!viewModel.CheckFileSaved()) return;
            
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