using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using BtfEditor.Core;
using BtfReader;

namespace BtfEditor.MVVM.ViewModel
{

    public class EditorViewModel: ObservableObject, IDisposable
    {
        public RelayCommand LoadFileCommand { get; set; } 
        
        private string _query;
        public string SearchQuery
        {
            get => _query;
            set
            {
                _query = value;
                Search(_query);
            }
        }

        public List<IBtfString> FilteredWritings { get; set; } = new List<IBtfString>();

        private IBtfString _selected;
        public IBtfString Selected
        {
            get => _selected;
            set
            {
                _selected = value;
                OnPropertyChanged();
            }
        }

        public event Action ItemsRefresh;

        public string Title => $"File: {Path.GetFileName(File.Path)} Total writings: {File.WritingsCount}";

        private BtfFile _file;
        public BtfFile File
        {
            get => _file;
            set
            {
                _file = value;
                FilteredWritings.AddRange(_file.Writings);
                Selected = _file.Writings[0];
                OnPropertyChanged();
            }
        }
        

        private CancellationTokenSource? _searchCancellationTokenSource;
        private Task _searchTask;
        
        private async void Search(string query)
        {
            Debug.WriteLine($"Search: {query}");
            _searchCancellationTokenSource?.Cancel();
            _searchCancellationTokenSource = new CancellationTokenSource();

            if (string.IsNullOrEmpty(query))
            {
                FilteredWritings.Clear();
                FilteredWritings.AddRange(_file.Writings);
                Selected = FilteredWritings.First();
                ItemsRefresh();
            }

            try
            {
                IList<IBtfString> filtered = new List<IBtfString>();

                Mouse.OverrideCursor = Cursors.Wait;
                _searchTask = Task.Run(() =>
                {
                    filtered = _file.Writings
                        .Where(x => x.Id.ToString().StartsWith(query))
                        .ToList();
                }, _searchCancellationTokenSource.Token);

                await _searchTask;
                if (_searchTask.IsCompletedSuccessfully) _searchCancellationTokenSource = null;

                if (filtered.Count <= 0) return;

                FilteredWritings.Clear();
                FilteredWritings.AddRange(filtered);
                Selected = FilteredWritings.First();
                ItemsRefresh();

                Debug.WriteLine($"SearchFinished: {FilteredWritings.Count}");

            }
            catch (TaskCanceledException)
            {
                _searchCancellationTokenSource = null;
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
            
        }

        public void SelectItem(int id)
        {
            Selected = _file.Writings.First(i => i.Id == id);
            Debug.WriteLine($"Selected id={Selected.Id} content={Selected.Content}");
        }

        public void SaveFile(string path)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            _file.SaveTo(path);
            Mouse.OverrideCursor = null;
        }
        
        public bool CheckFileSaved()
        {
            if (!File.IsChanged) return true;
            var result = MessageBox.Show("You have unsaved changes, continue?", "Warning",
                MessageBoxButton.YesNo);
            return result == MessageBoxResult.Yes;
        }
        
        public void Dispose()
        {
            _file.Dispose();
        }
    }
}