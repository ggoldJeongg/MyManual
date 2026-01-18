using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using MyManual.Commands;
using MyManual.Exceptions;
using MyManual.Models;
using MyManual.Services.Interfaces;
using MyManual.ViewModels.Base;

namespace MyManual.ViewModels
{
    public class OnboardingManageViewModel : ViewModelBase
    {
        // ==================== Services ====================

        private readonly IUserService _userService;
        private readonly IOnboardingService _onboardingService;
        private readonly IManualService _manualService;

        // ==================== 직원 목록 ====================

        private ObservableCollection<EmployeeItemViewModel> _employees = new();
        public ObservableCollection<EmployeeItemViewModel> Employees
        {
            get => _employees;
            set => SetProperty(ref _employees, value);
        }

        private EmployeeItemViewModel? _selectedEmployee;
        public EmployeeItemViewModel? SelectedEmployee
        {
            get => _selectedEmployee;
            set
            {
                if (SetProperty(ref _selectedEmployee, value))
                {
                    OnPropertyChanged(nameof(HasSelectedEmployee));
                    OnPropertyChanged(nameof(SelectedEmployeeAvatar));
                    OnPropertyChanged(nameof(SelectedEmployeeName));
                    OnPropertyChanged(nameof(SelectedEmployeeProgress));

                    if (value != null)
                    {
                        LoadTasksForEmployee();
                    }
                    ClearManualSelection();
                }
            }
        }

        public bool HasSelectedEmployee => SelectedEmployee != null;
        public string SelectedEmployeeAvatar => SelectedEmployee?.AvatarInitial ?? "";
        public string SelectedEmployeeName => SelectedEmployee?.Name ?? "";
        public string SelectedEmployeeProgress => SelectedEmployee != null ? $"진행률: {SelectedEmployee.ProgressText}" : "";

        public string EmployeeCountText => $"총 {Employees.Count}명";

        // ==================== 매뉴얼 목록 ====================

        private ObservableCollection<ManualItemViewModel> _manuals = new();
        public ObservableCollection<ManualItemViewModel> Manuals
        {
            get => _manuals;
            set => SetProperty(ref _manuals, value);
        }

        private ManualItemViewModel? _selectedManual;
        public ManualItemViewModel? SelectedManual
        {
            get => _selectedManual;
            set
            {
                if (SetProperty(ref _selectedManual, value))
                {
                    OnPropertyChanged(nameof(HasSelectedManual));
                    OnPropertyChanged(nameof(SelectedManualText));
                    OnPropertyChanged(nameof(CanAssign));
                }
            }
        }

        public bool HasSelectedManual => SelectedManual != null;
        public string SelectedManualText => SelectedManual?.Title ?? "매뉴얼을 선택하세요";
        public bool CanAssign => HasSelectedEmployee && HasSelectedManual;

        // ==================== 태스크 목록 ====================

        private ObservableCollection<DayTaskGroupViewModel> _dayTaskGroups = new();
        public ObservableCollection<DayTaskGroupViewModel> DayTaskGroups
        {
            get => _dayTaskGroups;
            set
            {
                if (SetProperty(ref _dayTaskGroups, value))
                {
                    OnPropertyChanged(nameof(HasTasks));
                }
            }
        }

        public bool HasTasks => DayTaskGroups.Count > 0;

        // ==================== Day 선택 ====================

        private int _selectedDay = 1;
        public int SelectedDay
        {
            get => _selectedDay;
            set => SetProperty(ref _selectedDay, value);
        }

        public List<int> AvailableDays { get; } = Enumerable.Range(1, 10).ToList();

        // ==================== Commands ====================

        public ICommand SelectEmployeeCommand { get; }
        public ICommand SelectManualCommand { get; }
        public ICommand AssignManualCommand { get; }
        public ICommand DeleteTaskCommand { get; }
        public ICommand ResetAllTasksCommand { get; }
        public ICommand RefreshCommand { get; }

        // ==================== 생성자 ====================

        public OnboardingManageViewModel(
            IUserService userService,
            IOnboardingService onboardingService,
            IManualService manualService)
        {
            _userService = userService;
            _onboardingService = onboardingService;
            _manualService = manualService;

            // Commands 초기화
            SelectEmployeeCommand = new RelayCommand(OnSelectEmployee);
            SelectManualCommand = new RelayCommand(OnSelectManual);
            AssignManualCommand = new RelayCommand(OnAssignManual, _ => CanAssign);
            DeleteTaskCommand = new RelayCommand(OnDeleteTask);
            ResetAllTasksCommand = new RelayCommand(OnResetAllTasks, _ => HasSelectedEmployee);
            RefreshCommand = new RelayCommand(_ => Refresh());

            // 데이터 로드
            LoadEmployees();
            LoadManuals();
        }

        // ==================== 데이터 로드 ====================

        private void LoadEmployees()
        {
            try
            {
                var allUsers = _userService.GetAllUsers();
                var newEmployees = allUsers.Where(u => !u.IsAdmin).ToList();

                var employeeVMs = newEmployees.Select(u =>
                {
                    var progress = _onboardingService.GetOverallProgress(u.Id);
                    return new EmployeeItemViewModel(u, progress.completed, progress.total);
                }).ToList();

                Employees = new ObservableCollection<EmployeeItemViewModel>(employeeVMs);
                OnPropertyChanged(nameof(EmployeeCountText));
            }
            catch (Exception ex)
            {
                ExceptionHandler.Handle(ex, "직원 목록 로드");
            }
        }

        private void LoadManuals()
        {
            try
            {
                var allManuals = _manualService.GetAllManuals();
                var manualVMs = allManuals.Select(m => new ManualItemViewModel
                {
                    ManualId = m.Id,
                    Title = m.Title,
                    Category = m.Category,
                    IsSelected = false
                }).ToList();

                Manuals = new ObservableCollection<ManualItemViewModel>(manualVMs);
            }
            catch (Exception ex)
            {
                ExceptionHandler.Handle(ex, "매뉴얼 목록 로드");
            }
        }

        private void LoadTasksForEmployee()
        {
            if (SelectedEmployee == null)
            {
                DayTaskGroups = new ObservableCollection<DayTaskGroupViewModel>();
                return;
            }

            try
            {
                var userTasks = _onboardingService.GetUserTasks(SelectedEmployee.UserId);

                if (userTasks.Count == 0)
                {
                    DayTaskGroups = new ObservableCollection<DayTaskGroupViewModel>();
                    return;
                }

                var dayGroups = userTasks
                    .GroupBy(t => t.Day)
                    .OrderBy(g => g.Key)
                    .Select(g =>
                    {
                        var tasks = g.Select(t =>
                        {
                            var isCompleted = _onboardingService.GetTaskStatus(SelectedEmployee.UserId, t.Id);
                            return new TaskItemViewModel
                            {
                                TaskId = t.Id,
                                Title = t.Title,
                                IsCompleted = isCompleted,
                                UserId = SelectedEmployee.UserId
                            };
                        }).ToList();

                        return new DayTaskGroupViewModel
                        {
                            Day = g.Key,
                            Tasks = tasks,
                            CompletedCount = tasks.Count(t => t.IsCompleted),
                            TotalCount = tasks.Count
                        };
                    }).ToList();

                DayTaskGroups = new ObservableCollection<DayTaskGroupViewModel>(dayGroups);
            }
            catch (Exception ex)
            {
                ExceptionHandler.Handle(ex, "태스크 목록 로드");
            }
        }

        // ==================== Commands 구현 ====================

        private void OnSelectEmployee(object? parameter)
        {
            if (parameter is EmployeeItemViewModel employee)
            {
                // 기존 선택 해제
                foreach (var emp in Employees)
                {
                    emp.IsSelected = false;
                }

                // 새로운 선택
                employee.IsSelected = true;
                SelectedEmployee = employee;
            }
        }

        private void OnSelectManual(object? parameter)
        {
            if (parameter is ManualItemViewModel manual)
            {
                // 기존 선택 해제
                foreach (var m in Manuals)
                {
                    m.IsSelected = false;
                }

                // 새로운 선택
                manual.IsSelected = true;
                SelectedManual = manual;
            }
        }

        private void ClearManualSelection()
        {
            foreach (var m in Manuals)
            {
                m.IsSelected = false;
            }
            _selectedManual = null;
            OnPropertyChanged(nameof(SelectedManual));
            OnPropertyChanged(nameof(HasSelectedManual));
            OnPropertyChanged(nameof(SelectedManualText));
            OnPropertyChanged(nameof(CanAssign));
        }

        private void OnAssignManual(object? parameter)
        {
            if (SelectedEmployee == null || SelectedManual == null) return;

            try
            {
                _onboardingService.AssignManualToUser(SelectedEmployee.UserId, SelectedManual.ManualId, SelectedDay);

                ExceptionHandler.ShowInfo($"'{SelectedManual.Title}' 매뉴얼이 {SelectedEmployee.Name}님의 Day {SelectedDay}에 분배되었습니다.");

                RefreshSelectedEmployee();
                ClearManualSelection();
            }
            catch (Exception ex)
            {
                ExceptionHandler.Handle(ex, "매뉴얼 분배");
            }
        }

        private void OnDeleteTask(object? parameter)
        {
            if (parameter is int taskId)
            {
                if (!ExceptionHandler.Confirm("이 할일을 삭제하시겠습니까?", "삭제 확인"))
                    return;

                try
                {
                    _onboardingService.DeleteTask(taskId);
                    RefreshSelectedEmployee();
                }
                catch (Exception ex)
                {
                    ExceptionHandler.Handle(ex, "태스크 삭제");
                }
            }
        }

        private void OnResetAllTasks(object? parameter)
        {
            if (SelectedEmployee == null) return;

            if (!ExceptionHandler.Confirm(
                $"{SelectedEmployee.Name}님의 모든 할일을 삭제하시겠습니까?\n이 작업은 되돌릴 수 없습니다.",
                "전체 초기화 확인"))
                return;

            try
            {
                int count = _onboardingService.DeleteAllUserTasks(SelectedEmployee.UserId);

                ExceptionHandler.ShowInfo($"{SelectedEmployee.Name}님의 {count}개 할일이 삭제되었습니다.");

                RefreshSelectedEmployee();
            }
            catch (Exception ex)
            {
                ExceptionHandler.Handle(ex, "태스크 초기화");
            }
        }

        private void RefreshSelectedEmployee()
        {
            if (SelectedEmployee == null) return;

            // 진행률 업데이트
            var progress = _onboardingService.GetOverallProgress(SelectedEmployee.UserId);
            SelectedEmployee.UpdateProgress(progress.completed, progress.total);
            OnPropertyChanged(nameof(SelectedEmployeeProgress));

            // 태스크 목록 갱신
            LoadTasksForEmployee();
        }

        public void Refresh()
        {
            LoadEmployees();
            LoadManuals();
            SelectedEmployee = null;
            SelectedManual = null;
            DayTaskGroups = new ObservableCollection<DayTaskGroupViewModel>();
        }
    }

    // ==================== 하위 ViewModel 클래스 ====================

    /// <summary>
    /// 직원 아이템 ViewModel
    /// </summary>
    public class EmployeeItemViewModel : ViewModelBase
    {
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string AvatarInitial { get; set; } = string.Empty;
        public string JoinDateText { get; set; } = string.Empty;

        private string _progressText = string.Empty;
        public string ProgressText
        {
            get => _progressText;
            set => SetProperty(ref _progressText, value);
        }

        private double _progressPercent;
        public double ProgressPercent
        {
            get => _progressPercent;
            set => SetProperty(ref _progressPercent, value);
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public EmployeeItemViewModel(User user, int completed, int total)
        {
            UserId = user.Id;
            Name = user.Name;
            AvatarInitial = user.Name.Length > 0 ? user.Name[0].ToString() : "?";
            JoinDateText = $"입사: {user.JoinDate:yyyy.MM.dd}";
            UpdateProgress(completed, total);
        }

        public void UpdateProgress(int completed, int total)
        {
            ProgressText = total > 0 ? $"{completed}/{total}" : "0/0";
            ProgressPercent = total > 0 ? (double)completed / total * 100 : 0;
        }
    }

    /// <summary>
    /// 매뉴얼 아이템 ViewModel
    /// </summary>
    public class ManualItemViewModel : ViewModelBase
    {
        public int ManualId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
    }

    /// <summary>
    /// Day별 태스크 그룹 ViewModel
    /// </summary>
    public class DayTaskGroupViewModel
    {
        public int Day { get; set; }
        public List<TaskItemViewModel> Tasks { get; set; } = new();
        public int CompletedCount { get; set; }
        public int TotalCount { get; set; }
    }

    /// <summary>
    /// 태스크 아이템 ViewModel
    /// </summary>
    public class TaskItemViewModel : ViewModelBase
    {
        public int TaskId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int UserId { get; set; }

        private bool _isCompleted;
        public bool IsCompleted
        {
            get => _isCompleted;
            set => SetProperty(ref _isCompleted, value);
        }
    }
}
