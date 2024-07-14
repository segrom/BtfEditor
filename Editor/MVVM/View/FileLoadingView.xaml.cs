using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Editor.MVVM.ViewModel;

namespace Editor.MVVM.View
{
    delegate void UpdateProgressBarDelegate(DependencyProperty dp, object value);
    public partial class FileLoadingView : UserControl
    {
        BackgroundWorker worker;
        private FileLoadingViewModel _viewModel;
        private CancellationTokenSource _tokenSource;

        private Stopwatch sw;
        public FileLoadingView()
        {
            _tokenSource = new CancellationTokenSource();
            InitializeComponent();
            worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(StartWork); 
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            Loaded -= OnLoaded;
            _viewModel = (FileLoadingViewModel)DataContext;
            
            worker.RunWorkerCompleted += WorkComplete;
            sw = Stopwatch.StartNew();
            worker.RunWorkerAsync();
        }
        
        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = null;
            _tokenSource.Cancel();
            _tokenSource.Dispose();
            worker.Dispose();
        }

        
        private void StartWork(object sender, DoWorkEventArgs e)
        {
            var progress = new Progress<float>();
            UpdateProgressBarDelegate updProgress = ProgressBar.SetValue;
            progress.ProgressChanged += (o, f) =>
            {
                Dispatcher.Invoke(updProgress, ProgressBar.ValueProperty, f * 100d);
            };
            _viewModel.Load(progress);
        }

        private async void WorkComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            Mouse.OverrideCursor = null;
            sw.Stop();
            Debug.WriteLine($"Sync loading end in {sw.Elapsed.TotalSeconds}s");
            await Task.Delay(20);
            ProgressBar.Visibility = Visibility.Hidden;
            Title.Text = "Load complete";
            _viewModel.OpenEditorCommand.Execute(_viewModel.Result!);
        }
    }
}