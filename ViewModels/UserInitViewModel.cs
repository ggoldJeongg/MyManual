using System;
using System.Windows.Input;
using MyManual.Commands;
using MyManual.Models.User;
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

            var user = new User
            {
                Id = 1,
                Name = Name,
                JoinDate = JoinDate!.Value
            };

            UserRegistered?.Invoke(user);
        }
    }
}
