using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using MyManual.Commands;
using MyManual.Exceptions;
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

        // ==================== 수정 모드 ====================

        private int? _editingManualId;
        private bool _isEditMode;

        public bool IsEditMode
        {
            get => _isEditMode;
            private set => SetProperty(ref _isEditMode, value);
        }

        public string PageTitle => IsEditMode ? "매뉴얼 수정" : "매뉴얼 입력";
        public string SubmitButtonText => IsEditMode ? "수정 완료" : "저장";

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
            SubmitCommand = new AsyncRelayCommand(_ => OnSubmitAsync(), _ => CanSubmit);
            CancelCommand = new RelayCommand(_ => OnCancel());
        }

        private async Task OnSubmitAsync()
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

                if (IsEditMode && _editingManualId.HasValue)
                {
                    // 수정 모드
                    var existingManual = await _manualService.GetManualByIdAsync(_editingManualId.Value);
                    if (existingManual == null)
                    {
                        ErrorMessage = "매뉴얼을 찾을 수 없습니다.";
                        return;
                    }

                    existingManual.Title = Title;
                    existingManual.Category = SelectedCategory ?? string.Empty;
                    existingManual.Purpose = Purpose;
                    existingManual.Process = Process;

                    // 체크리스트 업데이트
                    existingManual.Checklist.Clear();
                    if (!string.IsNullOrWhiteSpace(Checklist))
                    {
                        var items = Checklist.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var item in items)
                        {
                            existingManual.Checklist.Add(new ChecklistItem { Content = item.Trim() });
                        }
                    }

                    // 히스토리 추가 (수정 기록)
                    existingManual.History.Add(new HistoryItem
                    {
                        Date = DateTime.Now.ToString("yyyy-MM-dd"),
                        Description = !string.IsNullOrWhiteSpace(History) ? History : "매뉴얼 수정됨"
                    });

                    await _manualService.UpdateManualAsync(existingManual, currentUser);

                    System.Diagnostics.Debug.WriteLine($"[매뉴얼 수정] ID: {existingManual.Id}, Title: {existingManual.Title}");
                }
                else
                {
                    // 생성 모드
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

                    // 히스토리 추가 (입력한 경우에만)
                    if (!string.IsNullOrWhiteSpace(History))
                    {
                        manual.History.Add(new HistoryItem
                        {
                            Date = DateTime.Now.ToString("yyyy-MM-dd"),
                            Description = History
                        });
                    }

                    // DB에 저장 (비동기)
                    await _manualService.CreateManualAsync(manual, currentUser);

                    System.Diagnostics.Debug.WriteLine($"[매뉴얼 생성] ID: {manual.Id}, Title: {manual.Title}");
                }

                SubmitRequested?.Invoke();
            }
            catch (UnauthorizedException ex)
            {
                ErrorMessage = ex.UserMessage;
            }
            catch (ValidationException ex)
            {
                ErrorMessage = ex.UserMessage;
            }
            catch (EntityNotFoundException ex)
            {
                ErrorMessage = ex.UserMessage;
            }
            catch (Exception ex)
            {
                ErrorMessage = ExceptionHandler.GetUserMessage(ex, IsEditMode ? "매뉴얼 수정" : "매뉴얼 생성");
                System.Diagnostics.Debug.WriteLine($"[매뉴얼 {(IsEditMode ? "수정" : "생성")} 실패] {ex}");
            }
        }

        private void OnCancel()
        {
            CancelRequested?.Invoke();
        }

        public void Clear()
        {
            _editingManualId = null;
            IsEditMode = false;
            Title = string.Empty;
            SelectedCategory = null;
            Purpose = string.Empty;
            Process = string.Empty;
            Checklist = string.Empty;
            History = string.Empty;
            ErrorMessage = null;

            OnPropertyChanged(nameof(PageTitle));
            OnPropertyChanged(nameof(SubmitButtonText));
        }

        public async Task LoadForEditAsync(int manualId)
        {
            try
            {
                var manual = await _manualService.GetManualByIdAsync(manualId);
                if (manual == null)
                {
                    ErrorMessage = "매뉴얼을 찾을 수 없습니다.";
                    return;
                }

                _editingManualId = manualId;
                IsEditMode = true;

                Title = manual.Title;
                SelectedCategory = manual.Category;
                Purpose = manual.Purpose;
                Process = manual.Process;

                // 체크리스트를 줄바꿈으로 연결
                Checklist = string.Join("\n", manual.Checklist.Select(c => c.Content));

                // 히스토리 입력란은 비워둠 (수정 시 새 내용만 입력)
                History = string.Empty;
                ErrorMessage = null;

                OnPropertyChanged(nameof(PageTitle));
                OnPropertyChanged(nameof(SubmitButtonText));
            }
            catch (Exception ex)
            {
                ErrorMessage = ExceptionHandler.GetUserMessage(ex, "매뉴얼 로드");
            }
        }
    }
}
