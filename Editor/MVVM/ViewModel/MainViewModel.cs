using System.IO;
using BtfEditor.Core;
using BtfReader;

namespace BtfEditor.MVVM.ViewModel
{
    public class MainViewModel: ObservableObject
    {
        
        public RelayCommand LoadFileCommand { get; set; }
        public RelayCommand OpenEditorCommand { get; set; }
        
        public StartViewModel StartViewModel { get; set; }
        public FileLoadingViewModel FileLoadingViewModel { get; set; }
        public EditorViewModel EditorViewModel { get; set; }
        
        private object _currentView;
        public object CurrentView 
        {
            get
            {
                return _currentView;
            }
            set
            {
                _currentView = value;
                OnPropertyChanged();
            } 
        }

        public MainViewModel()
        {
            OpenEditorCommand = new RelayCommand(o =>
            {
                if (!(o is BtfFile btf)) return;
                EditorViewModel = new EditorViewModel()
                {
                    File = btf,
                    LoadFileCommand = LoadFileCommand
                };
                CurrentView = EditorViewModel;
            }, o => o is BtfFile);
            
            LoadFileCommand = new RelayCommand(o =>
            {
                if (!(o is string path) || !File.Exists(path)) return;
                FileLoadingViewModel = new FileLoadingViewModel()
                {
                    TargetPath = path,
                    OpenEditorCommand = OpenEditorCommand,
                };
                CurrentView = FileLoadingViewModel;
            }, o => o is string path && File.Exists(path));

            StartViewModel = new StartViewModel
            {
                LoadFileCommand = LoadFileCommand
            };
            CurrentView = StartViewModel;
        }
    }
}