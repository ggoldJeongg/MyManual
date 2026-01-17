using System;
using System.Windows.Input;
using MyManual.Commands;
using MyManual.Models;
using MyManual.ViewModels.Base;

namespace MyManual.ViewModels
{
    public class UserInitViewModel : ViewModelBase
    {
        private string _name = string.Empty;
        private DateTime? _joinDate;
        private string _errorMessage = string.Empty;

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanSubmit));
            }
        }

        public DateTime? JoinDate
        {
            get => _joinDate;
            set
            {
                _joinDate = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanSubmit));
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }

        public bool CanSubmit => !string.IsNullOrWhiteSpace(Name) && JoinDate.HasValue;

        public ICommand SubmitCommand { get; }

        // 등록 완료 이벤트
        public event Action<User>? UserRegistered;

        public UserInitViewModel()
        {
            SubmitCommand = new RelayCommand(_ => Submit(), _ => CanSubmit);
        }

        private void Submit()
        {
            if (!CanSubmit)
            {
                ErrorMessage = "이름과 입사일을 모두 입력해주세요.";
                return;
            }

            // Id는 지정하지 않음 - DB에서 자동 생성
            var user = new User
            {
                Name = Name,
                JoinDate = JoinDate!.Value,
                IsAdmin = false  // 기본값: 일반 사용자
            };

            UserRegistered?.Invoke(user);
        }
    }
}
