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
        private readonly IManualService _manualService;

        private List<NewEmployeeViewModel> _employees = new();
        private List<ManualItemViewModel> _manuals = new();
        private NewEmployeeViewModel? _selectedEmployee;
        private ManualItemViewModel? _selectedManual;

        public OnboardingManageView()
        {
            InitializeComponent();

            _userService = App.Services.GetRequiredService<IUserService>();
            _onboardingService = App.Services.GetRequiredService<IOnboardingService>();
            _manualService = App.Services.GetRequiredService<IManualService>();

            HeaderControl.OnboardingClick += (s, e) => NavigateToOnboarding?.Invoke();
            HeaderControl.ManualClick += (s, e) => NavigateToManual?.Invoke();
            HeaderControl.CategoryClick += (s, e) => NavigateToCategory?.Invoke();
            HeaderControl.UserNameClick += (s, e) => ProfileDrawer.Toggle();
            ProfileDrawer.LogoutRequested += OnLogoutRequested;

            LoadEmployees();
            LoadManuals();
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

        private void LoadManuals()
        {
            var allManuals = _manualService.GetAllManuals();
            _manuals = allManuals.Select(m => new ManualItemViewModel
            {
                ManualId = m.Id,
                Title = m.Title,
                Category = m.Category,
                IsSelected = false
            }).ToList();

            ManualListControl.ItemsSource = _manuals;
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

            // 매뉴얼 선택 초기화
            ClearManualSelection();

            // 할일 목록 로드
            LoadTasksForEmployee(employee);
        }

        private void LoadTasksForEmployee(NewEmployeeViewModel employee)
        {
            var userTasks = _onboardingService.GetUserTasks(employee.UserId);

            if (userTasks.Count == 0)
            {
                DayTasksControl.Visibility = Visibility.Collapsed;
                NoTasksMessage.Visibility = Visibility.Visible;
                return;
            }

            DayTasksControl.Visibility = Visibility.Visible;
            NoTasksMessage.Visibility = Visibility.Collapsed;

            // Day별로 그룹화
            var dayGroups = userTasks
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

        private void OnManualItemClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is ManualItemViewModel manual)
            {
                SelectManual(manual);
            }
        }

        private void SelectManual(ManualItemViewModel manual)
        {
            // 기존 선택 해제
            foreach (var m in _manuals)
            {
                m.IsSelected = false;
            }

            // 새로운 선택
            manual.IsSelected = true;
            _selectedManual = manual;

            // UI 업데이트
            ManualListControl.ItemsSource = null;
            ManualListControl.ItemsSource = _manuals;

            // 선택 표시 업데이트
            SelectedManualText.Text = manual.Title;
            SelectedManualText.Foreground = new System.Windows.Media.SolidColorBrush(
                (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#333"));
            AssignButton.IsEnabled = true;
        }

        private void ClearManualSelection()
        {
            foreach (var m in _manuals)
            {
                m.IsSelected = false;
            }
            _selectedManual = null;

            ManualListControl.ItemsSource = null;
            ManualListControl.ItemsSource = _manuals;

            SelectedManualText.Text = "매뉴얼을 선택하세요";
            SelectedManualText.Foreground = new System.Windows.Media.SolidColorBrush(
                (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#999"));
            AssignButton.IsEnabled = false;
        }

        private void OnAssignClick(object sender, RoutedEventArgs e)
        {
            if (_selectedEmployee == null || _selectedManual == null) return;

            try
            {
                // 선택된 Day 가져오기
                var selectedItem = DayComboBox.SelectedItem as ComboBoxItem;
                if (selectedItem == null) return;
                int day = int.Parse(selectedItem.Content.ToString()!);

                // 매뉴얼 분배
                _onboardingService.AssignManualToUser(_selectedEmployee.UserId, _selectedManual.ManualId, day);

                MessageBox.Show(
                    $"'{_selectedManual.Title}' 매뉴얼이 {_selectedEmployee.Name}님의 Day {day}에 분배되었습니다.",
                    "분배 완료",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                // 진행률 및 목록 갱신
                RefreshSelectedEmployee();
                ClearManualSelection();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"분배 중 오류 발생: {ex.Message}",
                    "오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void OnDeleteTaskClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int taskId)
            {
                var result = MessageBox.Show(
                    "이 할일을 삭제하시겠습니까?",
                    "삭제 확인",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes) return;

                try
                {
                    _onboardingService.DeleteTask(taskId);
                    RefreshSelectedEmployee();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"삭제 중 오류 발생: {ex.Message}",
                        "오류",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        private void OnResetClick(object sender, RoutedEventArgs e)
        {
            if (_selectedEmployee == null) return;

            var result = MessageBox.Show(
                $"{_selectedEmployee.Name}님의 모든 할일을 삭제하시겠습니까?\n이 작업은 되돌릴 수 없습니다.",
                "전체 초기화 확인",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                int count = _onboardingService.DeleteAllUserTasks(_selectedEmployee.UserId);

                MessageBox.Show(
                    $"{_selectedEmployee.Name}님의 {count}개 할일이 삭제되었습니다.",
                    "초기화 완료",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                RefreshSelectedEmployee();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"초기화 중 오류 발생: {ex.Message}",
                    "오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void RefreshSelectedEmployee()
        {
            if (_selectedEmployee == null) return;

            // 진행률 업데이트
            var progress = _onboardingService.GetOverallProgress(_selectedEmployee.UserId);
            _selectedEmployee.UpdateProgress(progress.completed, progress.total);
            SelectedUserProgress.Text = $"진행률: {_selectedEmployee.ProgressText}";

            // 목록 갱신
            UserListControl.ItemsSource = null;
            UserListControl.ItemsSource = _employees;

            // 태스크 목록 갱신
            LoadTasksForEmployee(_selectedEmployee);
        }

        public void Refresh()
        {
            LoadEmployees();
            LoadManuals();
            _selectedEmployee = null;
            _selectedManual = null;
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

    // 매뉴얼 아이템 ViewModel
    public class ManualItemViewModel : INotifyPropertyChanged
    {
        public int ManualId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;

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
