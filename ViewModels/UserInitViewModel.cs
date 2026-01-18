using System;
using System.Windows.Input;
using MyManual.Commands;
using MyManual.Models;
using MyManual.Services.Interfaces;
using MyManual.ViewModels.Base;

namespace MyManual.ViewModels
{
    public class UserInitViewModel : ViewModelBase
    {
        private readonly IUserService _userService;

        private string _name = string.Empty;
        private DateTime? _joinDate;
        private string _password = string.Empty;
        private string _passwordConfirm = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _isLoginMode = true;

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

        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanSubmit));
            }
        }

        public string PasswordConfirm
        {
            get => _passwordConfirm;
            set
            {
                _passwordConfirm = value;
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

        public bool IsLoginMode
        {
            get => _isLoginMode;
            set
            {
                _isLoginMode = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsRegisterMode));
                OnPropertyChanged(nameof(SubmitButtonText));
                OnPropertyChanged(nameof(CanSubmit));
                ErrorMessage = string.Empty;
            }
        }

        public bool IsRegisterMode => !_isLoginMode;

        public string SubmitButtonText => IsLoginMode ? "로그인" : "회원가입";

        public bool CanSubmit
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Name)) return false;
                if (string.IsNullOrWhiteSpace(Password)) return false;

                if (IsRegisterMode)
                {
                    if (!JoinDate.HasValue) return false;
                    if (string.IsNullOrWhiteSpace(PasswordConfirm)) return false;
                }

                return true;
            }
        }

        public ICommand SubmitCommand { get; }
        public ICommand SwitchToLoginCommand { get; }
        public ICommand SwitchToRegisterCommand { get; }

        // 등록/로그인 완료 이벤트
        public event Action<User>? UserRegistered;

        public UserInitViewModel()
        {
            _userService = App.GetService<IUserService>();

            SubmitCommand = new RelayCommand(_ => Submit(), _ => CanSubmit);
            SwitchToLoginCommand = new RelayCommand(_ => IsLoginMode = true);
            SwitchToRegisterCommand = new RelayCommand(_ => IsLoginMode = false);
        }

        private void Submit()
        {
            ErrorMessage = string.Empty;

            if (!CanSubmit)
            {
                ErrorMessage = IsLoginMode
                    ? "이름과 비밀번호를 입력해주세요."
                    : "모든 항목을 입력해주세요.";
                return;
            }

            if (IsLoginMode)
            {
                Login();
            }
            else
            {
                Register();
            }
        }

        private void Login()
        {
            var user = _userService.ValidateLogin(Name, Password);
            if (user == null)
            {
                ErrorMessage = "이름 또는 비밀번호가 일치하지 않습니다.";
                return;
            }

            UserRegistered?.Invoke(user);
        }

        private void Register()
        {
            // 비밀번호 확인
            if (Password != PasswordConfirm)
            {
                ErrorMessage = "비밀번호가 일치하지 않습니다.";
                return;
            }

            // 비밀번호 길이 검증
            if (Password.Length < 4)
            {
                ErrorMessage = "비밀번호는 최소 4자 이상이어야 합니다.";
                return;
            }

            // 이미 존재하는 사용자 확인
            if (_userService.UserExists(Name))
            {
                ErrorMessage = "이미 등록된 이름입니다. 로그인해주세요.";
                return;
            }

            // 사용자 생성
            var user = _userService.CreateUser(Name, JoinDate!.Value, Password);
            UserRegistered?.Invoke(user);
        }
    }
}
