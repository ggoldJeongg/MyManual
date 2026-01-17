using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using MyManual.Commands;
using MyManual.Models;
using MyManual.Services;
using MyManual.Services.Interfaces;
using MyManual.ViewModels.Base;

namespace MyManual.ViewModels
{
    public class ManualCreateViewModel : ViewModelBase
    {
        // ==================== Service ====================

        private readonly IManualService _manualService;

        // ==================== 데이터 ====================

        private string _title = string.Empty;
        private string? _selectedCategory;
        private string _purpose = string.Empty;
        private string _process = string.Empty;
        private string _checklist = string.Empty;
        private string _history = string.Empty;
        private string? _errorMessage;

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

        public string? ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public ICommand SubmitCommand { get; }
        public ICommand CancelCommand { get; }

        // 이벤트
        public event Action? SubmitRequested;
        public event Action? CancelRequested;

        public ManualCreateViewModel(IManualService manualService)
        {
            _manualService = manualService;
            SubmitCommand = new RelayCommand(_ => OnSubmit(), _ => CanSubmit);
            CancelCommand = new RelayCommand(_ => OnCancel());
        }

        private void OnSubmit()
        {
            ErrorMessage = null;

            try
            {
                var currentUser = App.CurrentUser;
                if (currentUser == null)
                {
                    ErrorMessage = "로그인이 필요합니다.";
                    return;
                }

                // Manual 객체 생성
                var manual = new Manual
                {
                    Title = Title,
                    Category = SelectedCategory ?? string.Empty,
                    Purpose = Purpose,
                    Process = Process
                };

                // 체크리스트 파싱 (줄바꿈으로 구분)
                if (!string.IsNullOrWhiteSpace(Checklist))
                {
                    var items = Checklist.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var item in items)
                    {
                        manual.Checklist.Add(new ChecklistItem { Content = item.Trim() });
                    }
                }

                // 히스토리 추가 (생성 기록)
                if (!string.IsNullOrWhiteSpace(History))
                {
                    manual.History.Add(new HistoryItem
                    {
                        Date = DateTime.Now.ToString("yyyy-MM-dd"),
                        Description = History
                    });
                }
                else
                {
                    manual.History.Add(new HistoryItem
                    {
                        Date = DateTime.Now.ToString("yyyy-MM-dd"),
                        Description = "매뉴얼 생성됨"
                    });
                }

                // DB에 저장
                _manualService.CreateManual(manual, currentUser);

                System.Diagnostics.Debug.WriteLine($"[매뉴얼 생성] ID: {manual.Id}, Title: {manual.Title}");

                SubmitRequested?.Invoke();
            }
            catch (UnauthorizedAccessException ex)
            {
                ErrorMessage = ex.Message;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"저장 실패: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"[매뉴얼 생성 실패] {ex}");
            }
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
