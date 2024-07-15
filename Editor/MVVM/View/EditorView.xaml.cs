using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BtfEditor.MVVM.ViewModel;
using BtfReader;
using Microsoft.Win32;

namespace BtfEditor.MVVM.View
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
            viewModel.File.PropertyChanged += (o, args) =>
            {
                EditedText.Visibility = viewModel.File.IsChanged ? Visibility.Visible : Visibility.Hidden;
            };
            EditedText.Visibility = viewModel.File.IsChanged ? Visibility.Visible : Visibility.Hidden;
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
            if(sender is ScrollViewer scv)
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