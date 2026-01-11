using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using MyManual.Commands;
using MyManual.ViewModels.Base;

namespace MyManual.ViewModels
{
    public class ManualCreateViewModel : ViewModelBase
    {
        private string _title = string.Empty;
        private string? _selectedCategory;
        private string _purpose = string.Empty;
        private string _process = string.Empty;
        private string _checklist = string.Empty;
        private string _history = string.Empty;

        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanSubmit));
            }
        }

        public string? SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanSubmit));
            }
        }

        public string Purpose
        {
            get => _purpose;
            set
            {
                _purpose = value;
                OnPropertyChanged();
            }
        }

        public string Process
        {
            get => _process;
            set
            {
                _process = value;
                OnPropertyChanged();
            }
        }

        public string Checklist
        {
            get => _checklist;
            set
            {
                _checklist = value;
                OnPropertyChanged();
            }
        }

        public string History
        {
            get => _history;
            set
            {
                _history = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<string> Categories { get; } = new()
        {
            "사내 규칙",
            "업무도구 / 시스템",
            "실무 프로세스",
            "복지 / 생활가이드"
        };

        public string CurrentUserName => App.CurrentUser?.Name + "님" ?? "";

        public bool CanSubmit => !string.IsNullOrWhiteSpace(Title) && !string.IsNullOrEmpty(SelectedCategory);

        public ICommand SubmitCommand { get; }
        public ICommand CancelCommand { get; }

        // 이벤트
        public event Action? SubmitRequested;
        public event Action? CancelRequested;

        public ManualCreateViewModel()
        {
            SubmitCommand = new RelayCommand(_ => OnSubmit(), _ => CanSubmit);
            CancelCommand = new RelayCommand(_ => OnCancel());
        }

        private void OnSubmit()
        {
            // TODO: DB 연결 후 저장 로직 구현
            SubmitRequested?.Invoke();
        }

        private void OnCancel()
        {
            CancelRequested?.Invoke();
        }

        public void Clear()
        {
            Title = string.Empty;
            SelectedCategory = null;
            Purpose = string.Empty;
            Process = string.Empty;
            Checklist = string.Empty;
            History = string.Empty;
        }
    }
}
