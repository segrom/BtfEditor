using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using btfReader;
using Editor.Core;

namespace Editor.MVVM.ViewModel
{
    public class FileLoadingViewModel: ObservableObject
    {
        public string Subtitle => $"{Path.GetFileName(TargetPath)}";
        public string TargetPath { get; set; }
        public BtfFile? Result { get; set; }
        public RelayCommand OpenEditorCommand { get; set; } 

        public async Task LoadAsync(IProgress<float> progress, CancellationToken token = default)
        {
            Result = new BtfFile(TargetPath);
            await Result.LoadAsync(progress, token);
        }

        public void Load(IProgress<float> progress)
        {
            Result = new BtfFile(TargetPath);
            Result.Load(progress);
        }
    }
}