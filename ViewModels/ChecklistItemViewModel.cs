using MyManual.ViewModels.Base;

namespace MyManual.ViewModels
{
    /// <summary>
    /// 체크리스트 항목 + 사용자별 체크 상태를 담는 ViewModel
    /// </summary>
    public class ChecklistItemViewModel : ViewModelBase
    {
        public int Id { get; set; }
        public int ManualId { get; set; }
        public string Content { get; set; } = string.Empty;
        public int OrderIndex { get; set; }

        private bool _isChecked;
        public bool IsChecked
        {
            get => _isChecked;
            set => SetProperty(ref _isChecked, value);
        }
    }
}
