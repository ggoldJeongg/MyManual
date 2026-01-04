using MyManual.Commands;
using MyManual.Models.User;
using MyManual.ViewModels.Base;
using MyManual.Helpers;
using MyManual.Models.Onboarding;

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace MyManual.ViewModels
{
    public class OnboardingViewModel : ViewModelBase
    {
        // ==================== 데이터 ====================

        //private User? _currentUser;
        public User CurrentUser { get; }

        private int _currentDay;
        public int CurrentDay
        {
            get => _currentDay;
            set
            {
                SetProperty(ref _currentDay, value);
                LoadTasksForCurrentDay();  // Day 바뀌면 작업 다시 로드
            }
        }

        private int _completedCount;
        public int CompletedCount
        {
            get => _completedCount;
            set
            {
                SetProperty(ref _completedCount, value);
                OnPropertyChanged(nameof(ProgressPercentage));
                OnPropertyChanged(nameof(ProgressText));
            }
        }

        private int _totalCount;
        public int TotalCount
        {
            get => _totalCount;
            set
            {
                SetProperty(ref _totalCount, value);
                OnPropertyChanged(nameof(ProgressPercentage));
                OnPropertyChanged(nameof(ProgressText));
            }
        }

        // 계산된 속성
        public double ProgressPercentage => TotalCount > 0
            ? (double)CompletedCount / TotalCount * 100
            : 0;

        public string ProgressText => $"{CompletedCount}/{TotalCount} 완료 ({ProgressPercentage:F0}%)";

        // 입사 정보
        public string JoinDateText => $"입사일: {CurrentUser?.JoinDate:yyyy년 MM월 dd일}";
        public string CurrentDayText => $"DAY {CurrentDay}";

        private ObservableCollection<OnboardingTask>? _tasks;
        public ObservableCollection<OnboardingTask> Tasks
        {
            get => _tasks ??= new ObservableCollection<OnboardingTask>();
            set => SetProperty(ref _tasks, value);
        }

        // ==================== Commands ====================

        public ICommand ToggleTaskCommand { get; }  // 체크박스 토글
        public ICommand OpenManualCommand { get; }  // 매뉴얼 열기
        public ICommand NextDayCommand { get; }
        public ICommand PreviousDayCommand { get; }

        // ==================== 생성자 ====================

        public OnboardingViewModel(User user)
        {
            // 현재 사용자 로드 (임시 데이터)
            CurrentUser = user;

            // 현재 Day 계산
            CurrentDay = WorkingDayCalculator.GetWorkingDay(CurrentUser.JoinDate);

            // 작업 로드
            LoadTasksForCurrentDay();

            // Command 초기화
            ToggleTaskCommand = new RelayCommand(OnToggleTask);
            OpenManualCommand = new RelayCommand(OnOpenManual, CanOpenManual);
            NextDayCommand = new RelayCommand(OnNextDay, CanGoNextDay);
            PreviousDayCommand = new RelayCommand(OnPreviousDay, CanGoPreviousDay);
        }

        // ==================== 메서드 ====================

        private void LoadTasksForCurrentDay()
        {
            // 실제로는 DB나 Service에서 로드
            // 여기서는 임시 데이터

            Tasks = new ObservableCollection<OnboardingTask>();

            // Day별 작업 정의
            if (CurrentDay == 1)
            {
                Tasks.Add(new OnboardingTask
                {
                    Id = 1,
                    Title = "사내 메일 할당받기",
                    IsCompleted = false,
                    ManualId = 101  // 매뉴얼 ID
                });
                Tasks.Add(new OnboardingTask
                {
                    Id = 2,
                    Title = "PC 및 업무 장비 수령",
                    IsCompleted = false,
                    ManualId = 102
                });
                Tasks.Add(new OnboardingTask
                {
                    Id = 3,
                    Title = "사내 시스템 계정 발급",
                    IsCompleted = false,
                    ManualId = 103
                });
                Tasks.Add(new OnboardingTask
                {
                    Id = 4,
                    Title = "팀원 소개 받기",
                    IsCompleted = false,
                    ManualId = 104
                });
                Tasks.Add(new OnboardingTask
                {
                    Id = 5,
                    Title = "회사 규정 숙지",
                    IsCompleted = false,
                    ManualId = 105
                });
            }
            else if (CurrentDay == 2)
            {
                Tasks.Add(new OnboardingTask
                {
                    Id = 6,
                    Title = "프로젝트 현황 파악",
                    IsCompleted = false,
                    ManualId = 201
                });
                // ... 더 추가
            }
            // Day 3, 4, 5도 마찬가지...

            UpdateCounts();
        }

        private void UpdateCounts()
        {
            CompletedCount = Tasks.Count(t => t.IsCompleted);
            TotalCount = Tasks.Count;
        }

        // 체크박스 토글
        private void OnToggleTask(object? parameter)
        {
            if (parameter is OnboardingTask task)
            {
                task.IsCompleted = !task.IsCompleted;
                UpdateCounts();

                // 모두 완료 시
                if (CompletedCount == TotalCount)
                {
                    // TODO: 축하 메시지 또는 자동으로 다음 Day
                    // MessageBox.Show("오늘 할 일을 모두 완료했습니다! 👏");
                }
            }
        }

        // 매뉴얼 열기
        private void OnOpenManual(object? parameter)
        {
            if (parameter is OnboardingTask task)
            {
                // 매뉴얼 뷰로 이동 (ManualId 전달)
                // 방법 1: 새 창 열기
                // var manualView = new ManualDetailView(task.ManualId);
                // manualView.Show();

                // 방법 2: 페이지 전환 (나중에 구현)
                // NavigationService.Navigate(new ManualDetailView(task.ManualId));

                // 임시: 콘솔 출력
                Console.WriteLine($"매뉴얼 열기: {task.Title} (ID: {task.ManualId})");
            }
        }

        private bool CanOpenManual(object? parameter)
        {
            return parameter is OnboardingTask;
        }

        private void OnNextDay(object? parameter)
        {
            CurrentDay++;
        }

        private bool CanGoNextDay(object? parameter)
        {
            // 모두 완료해야 다음 Day 가능
            return CompletedCount == TotalCount && CurrentDay < 5;
        }

        private void OnPreviousDay(object? parameter)
        {
            CurrentDay--;
        }

        private bool CanGoPreviousDay(object? parameter)
        {
            return CurrentDay > 1;
        }
    }
}