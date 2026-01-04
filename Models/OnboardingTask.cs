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
            set => SetProperty(ref _isCompleted, value);
        }

        public string StatusText => IsCompleted ? "완료" : "진행중";
    }
}
