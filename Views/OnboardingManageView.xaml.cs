using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using MyManual.Models;
using MyManual.Services.Interfaces;

namespace MyManual.Views
{
    public partial class OnboardingManageView : UserControl
    {
        public event Action? NavigateToOnboarding;
        public event Action? NavigateToManual;
        public event Action? NavigateToCategory;

        private readonly IUserService _userService;
        private readonly IOnboardingService _onboardingService;

        private List<NewEmployeeViewModel> _employees = new();
        private NewEmployeeViewModel? _selectedEmployee;

        public OnboardingManageView()
        {
            InitializeComponent();

            _userService = App.Services.GetRequiredService<IUserService>();
            _onboardingService = App.Services.GetRequiredService<IOnboardingService>();

            HeaderControl.OnboardingClick += (s, e) => NavigateToOnboarding?.Invoke();
            HeaderControl.ManualClick += (s, e) => NavigateToManual?.Invoke();
            HeaderControl.CategoryClick += (s, e) => NavigateToCategory?.Invoke();
            HeaderControl.UserNameClick += (s, e) => ProfileDrawer.Toggle();
            ProfileDrawer.LogoutRequested += OnLogoutRequested;

            LoadEmployees();
        }

        private void OnLogoutRequested(object? sender, EventArgs e)
        {
            var navigationService = App.Services.GetRequiredService<INavigationService>();
            navigationService.NavigateToUserInit();
        }

        private void LoadEmployees()
        {
            var allUsers = _userService.GetAllUsers();

            // 관리자가 아닌 사용자(신입사원)만 필터링
            var newEmployees = allUsers.Where(u => !u.IsAdmin).ToList();

            _employees = newEmployees.Select(u =>
            {
                var progress = _onboardingService.GetOverallProgress(u.Id);
                return new NewEmployeeViewModel(u, progress.completed, progress.total);
            }).ToList();

            UserListControl.ItemsSource = _employees;
            UserCountText.Text = $"총 {_employees.Count}명";
        }

        private void OnUserItemClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is NewEmployeeViewModel employee)
            {
                SelectEmployee(employee);
            }
        }

        private void SelectEmployee(NewEmployeeViewModel employee)
        {
            // 기존 선택 해제
            foreach (var emp in _employees)
            {
                emp.IsSelected = false;
            }

            // 새로운 선택
            employee.IsSelected = true;
            _selectedEmployee = employee;

            // UI 업데이트
            UserListControl.ItemsSource = null;
            UserListControl.ItemsSource = _employees;

            // 오른쪽 패널 표시
            NoSelectionPanel.Visibility = Visibility.Collapsed;
            TaskPanel.Visibility = Visibility.Visible;

            // 선택된 사용자 정보 표시
            SelectedUserAvatar.Text = employee.AvatarInitial;
            SelectedUserName.Text = employee.Name;
            SelectedUserProgress.Text = $"진행률: {employee.ProgressText}";

            // 할일 목록 로드
            LoadTasksForEmployee(employee);
        }

        private void LoadTasksForEmployee(NewEmployeeViewModel employee)
        {
            var allTasks = _onboardingService.GetAllTasks();

            // Day별로 그룹화
            var dayGroups = allTasks
                .GroupBy(t => t.Day)
                .OrderBy(g => g.Key)
                .Select(g =>
                {
                    var tasks = g.Select(t =>
                    {
                        var isCompleted = _onboardingService.GetTaskStatus(employee.UserId, t.Id);
                        return new TaskItemViewModel
                        {
                            TaskId = t.Id,
                            Title = t.Title,
                            IsCompleted = isCompleted,
                            UserId = employee.UserId
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

            DayTasksControl.ItemsSource = dayGroups;
        }

        private void OnTaskCheckChanged(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.Tag is TaskItemViewModel task)
            {
                // DB 업데이트
                _onboardingService.SetTaskStatus(task.UserId, task.TaskId, task.IsCompleted);

                // 진행률 업데이트
                if (_selectedEmployee != null)
                {
                    var progress = _onboardingService.GetOverallProgress(_selectedEmployee.UserId);
                    _selectedEmployee.UpdateProgress(progress.completed, progress.total);
                    SelectedUserProgress.Text = $"진행률: {_selectedEmployee.ProgressText}";

                    // 목록 갱신
                    UserListControl.ItemsSource = null;
                    UserListControl.ItemsSource = _employees;

                    // 태스크 목록 갱신
                    LoadTasksForEmployee(_selectedEmployee);
                }
            }
        }

        public void Refresh()
        {
            LoadEmployees();
            _selectedEmployee = null;
            NoSelectionPanel.Visibility = Visibility.Visible;
            TaskPanel.Visibility = Visibility.Collapsed;
        }
    }

    // 신입사원 ViewModel
    public class NewEmployeeViewModel : INotifyPropertyChanged
    {
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string AvatarInitial { get; set; } = string.Empty;
        public string JoinDateText { get; set; } = string.Empty;
        public string ProgressText { get; set; } = string.Empty;
        public double ProgressPercent { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public NewEmployeeViewModel(User user, int completed, int total)
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
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ProgressText)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ProgressPercent)));
        }
    }

    // Day별 태스크 그룹 ViewModel
    public class DayTaskGroupViewModel
    {
        public int Day { get; set; }
        public List<TaskItemViewModel> Tasks { get; set; } = new();
        public int CompletedCount { get; set; }
        public int TotalCount { get; set; }
    }

    // 태스크 아이템 ViewModel
    public class TaskItemViewModel : INotifyPropertyChanged
    {
        public int TaskId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int UserId { get; set; }

        private bool _isCompleted;
        public bool IsCompleted
        {
            get => _isCompleted;
            set
            {
                _isCompleted = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsCompleted)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
