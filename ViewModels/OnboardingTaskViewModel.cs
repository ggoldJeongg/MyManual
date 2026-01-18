using MyManual.ViewModels.Base;

namespace MyManual.ViewModels
{
    /// <summary>
    /// 온보딩 태스크 표시용 ViewModel (UI 바인딩용)
    /// </summary>
    public class OnboardingTaskViewModel : ViewModelBase
    {
        private bool _isCompleted;

        public int Id { get; set; }
        public int Day { get; set; }
        public string Title { get; set; } = string.Empty;
        public int ManualId { get; set; }

        public bool IsCompleted
        {
            get => _isCompleted;
            set
            {
                if (SetProperty(ref _isCompleted, value))
                {
                    OnPropertyChanged(nameof(StatusText));
                }
            }
        }

        public string StatusText => IsCompleted ? "완료" : "진행중";
    }
}
