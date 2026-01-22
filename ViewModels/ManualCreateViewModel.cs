using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using MyManual.Commands;
using MyManual.Data;
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
        private readonly IImageService? _imageService;

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
        private string _newHistoryDescription = string.Empty;
        private DateTime? _newHistoryDate = DateTime.Today;
        private string? _errorMessage;

        // 기존 히스토리 목록 (수정/삭제 가능)
        private ObservableCollection<EditableHistoryItem> _historyItems = new();
        public ObservableCollection<EditableHistoryItem> HistoryItems
        {
            get => _historyItems;
            set => SetProperty(ref _historyItems, value);
        }

        // 이미지 목록
        private ObservableCollection<EditableImageItem> _images = new();
        public ObservableCollection<EditableImageItem> Images
        {
            get => _images;
            set => SetProperty(ref _images, value);
        }

        private bool _isUploading;
        public bool IsUploading
        {
            get => _isUploading;
            set => SetProperty(ref _isUploading, value);
        }

        public bool HasImageService => _imageService != null;

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

        public string NewHistoryDescription
        {
            get => _newHistoryDescription;
            set
            {
                _newHistoryDescription = value;
                OnPropertyChanged();
            }
        }

        public DateTime? NewHistoryDate
        {
            get => _newHistoryDate;
            set
            {
                _newHistoryDate = value;
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
        public ICommand AddHistoryCommand { get; }
        public ICommand DeleteHistoryCommand { get; }
        public ICommand AddImageCommand { get; }
        public ICommand DeleteImageCommand { get; }

        // 이벤트
        public event Action? SubmitRequested;
        public event Action? CancelRequested;
        public event Func<Task<List<string>>>? ImageSelectRequested;

        public ManualCreateViewModel(IManualService manualService)
        {
            _manualService = manualService;
            _imageService = App.Services.GetService<IImageService>();

            SubmitCommand = new AsyncRelayCommand(_ => OnSubmitAsync(), _ => CanSubmit);
            CancelCommand = new RelayCommand(_ => OnCancel());
            AddHistoryCommand = new RelayCommand(_ => OnAddHistory());
            DeleteHistoryCommand = new RelayCommand(OnDeleteHistory);
            AddImageCommand = new AsyncRelayCommand(_ => OnAddImageAsync());
            DeleteImageCommand = new RelayCommand(OnDeleteImage);
        }

        private void OnAddHistory()
        {
            if (string.IsNullOrWhiteSpace(NewHistoryDescription)) return;

            HistoryItems.Add(new EditableHistoryItem
            {
                Date = NewHistoryDate?.ToString("yyyy-MM-dd") ?? DateTime.Today.ToString("yyyy-MM-dd"),
                Description = NewHistoryDescription,
                IsNew = true
            });

            // 입력란 초기화
            NewHistoryDescription = string.Empty;
            NewHistoryDate = DateTime.Today;
        }

        private void OnDeleteHistory(object? parameter)
        {
            if (parameter is EditableHistoryItem item)
            {
                HistoryItems.Remove(item);
            }
        }

        private async Task OnAddImageAsync()
        {
            if (_imageService == null)
            {
                ErrorMessage = "이미지 서비스가 설정되지 않았습니다.";
                return;
            }

            try
            {
                // View에서 파일 선택 다이얼로그를 통해 파일 경로 목록을 받아옴
                if (ImageSelectRequested == null) return;

                var filePaths = await ImageSelectRequested.Invoke();
                if (filePaths == null || filePaths.Count == 0) return;

                IsUploading = true;
                ErrorMessage = null;

                foreach (var filePath in filePaths)
                {
                    try
                    {
                        var fileName = Path.GetFileName(filePath);
                        using var fileStream = File.OpenRead(filePath);

                        var blobUrl = await _imageService.UploadImageAsync(fileStream, fileName);

                        Images.Add(new EditableImageItem
                        {
                            BlobUrl = blobUrl,
                            FileName = fileName,
                            IsNew = true,
                            OrderIndex = Images.Count
                        });

                        System.Diagnostics.Debug.WriteLine($"[이미지 추가] {fileName} -> {blobUrl}");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[이미지 업로드 실패] {filePath}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "이미지 업로드 중 오류가 발생했습니다.";
                System.Diagnostics.Debug.WriteLine($"[OnAddImageAsync] {ex}");
            }
            finally
            {
                IsUploading = false;
            }
        }

        private async void OnDeleteImage(object? parameter)
        {
            if (parameter is EditableImageItem item)
            {
                // 새로 추가된 이미지는 Blob에서도 삭제
                if (item.IsNew && _imageService != null && !string.IsNullOrEmpty(item.BlobUrl))
                {
                    await _imageService.DeleteImageAsync(item.BlobUrl);
                }

                Images.Remove(item);

                // OrderIndex 재정렬
                for (int i = 0; i < Images.Count; i++)
                {
                    Images[i].OrderIndex = i;
                }
            }
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

                    // 히스토리 동기화: 기존 히스토리 업데이트
                    existingManual.History.Clear();
                    foreach (var item in HistoryItems)
                    {
                        existingManual.History.Add(new HistoryItem
                        {
                            Id = item.OriginalId ?? 0,
                            ManualId = _editingManualId.Value,
                            Date = item.Date,
                            Description = item.Description
                        });
                    }

                    // 이미지 동기화
                    existingManual.Images.Clear();
                    foreach (var img in Images)
                    {
                        existingManual.Images.Add(new ManualImage
                        {
                            Id = img.OriginalId ?? 0,
                            ManualId = _editingManualId.Value,
                            BlobUrl = img.BlobUrl,
                            FileName = img.FileName,
                            OrderIndex = img.OrderIndex
                        });
                    }

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

                    // 히스토리 추가
                    foreach (var item in HistoryItems)
                    {
                        manual.History.Add(new HistoryItem
                        {
                            Date = item.Date,
                            Description = item.Description
                        });
                    }

                    // 이미지 추가
                    foreach (var img in Images)
                    {
                        manual.Images.Add(new ManualImage
                        {
                            BlobUrl = img.BlobUrl,
                            FileName = img.FileName,
                            OrderIndex = img.OrderIndex
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
            NewHistoryDescription = string.Empty;
            NewHistoryDate = DateTime.Today;
            HistoryItems.Clear();
            Images.Clear();
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

                // 기존 히스토리 로드
                HistoryItems.Clear();
                foreach (var history in manual.History.OrderBy(h => h.Date))
                {
                    HistoryItems.Add(new EditableHistoryItem
                    {
                        OriginalId = history.Id,
                        Date = history.Date,
                        Description = history.Description,
                        IsNew = false
                    });
                }

                // 기존 이미지 로드
                Images.Clear();
                foreach (var image in manual.Images.OrderBy(i => i.OrderIndex))
                {
                    Images.Add(new EditableImageItem
                    {
                        OriginalId = image.Id,
                        BlobUrl = image.BlobUrl,
                        FileName = image.FileName,
                        OrderIndex = image.OrderIndex,
                        IsNew = false
                    });
                }

                // 새 히스토리 입력란 초기화
                NewHistoryDescription = string.Empty;
                NewHistoryDate = DateTime.Today;
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

    // 편집 가능한 히스토리 아이템
    public class EditableHistoryItem : ViewModelBase
    {
        public int? OriginalId { get; set; }

        private string _date = string.Empty;
        public string Date
        {
            get => _date;
            set => SetProperty(ref _date, value);
        }

        private string _description = string.Empty;
        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public bool IsNew { get; set; }
    }

    // 편집 가능한 이미지 아이템
    public class EditableImageItem : ViewModelBase
    {
        public int? OriginalId { get; set; }

        private string _blobUrl = string.Empty;
        public string BlobUrl
        {
            get => _blobUrl;
            set => SetProperty(ref _blobUrl, value);
        }

        private string _fileName = string.Empty;
        public string FileName
        {
            get => _fileName;
            set => SetProperty(ref _fileName, value);
        }

        private int _orderIndex;
        public int OrderIndex
        {
            get => _orderIndex;
            set => SetProperty(ref _orderIndex, value);
        }

        public bool IsNew { get; set; }
    }
}
