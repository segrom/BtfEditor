using Editor.Core;

namespace Editor.MVVM.ViewModel
{
    public class StartViewModel: ObservableObject
    {
        public RelayCommand LoadFileCommand { get; set; } 

    }
}