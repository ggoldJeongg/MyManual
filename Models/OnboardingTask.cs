using MyManual.ViewModels.Base; // ViewModelBase ����

namespace MyManual.Models.Onboarding
{
    public class OnboardingTask : ViewModelBase
    {
        private bool _isCompleted;

        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int ManualId { get; set; }

public bool IsCompleted
        {
            get => _isCompleted;
            set
            {
                if (SetProperty(ref _isCompleted, value))
                {
                    // IsCompleted가 바뀌면 StatusText도 UI에 알려줘야 함
                    OnPropertyChanged(nameof(StatusText));
                }
            }
        }

        public string StatusText => IsCompleted ? "완료" : "진행중";
    }
}
