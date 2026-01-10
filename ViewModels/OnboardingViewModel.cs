using MyManual.Commands;
using MyManual.Models.User;
using MyManual.ViewModels.Base;
using MyManual.Helpers;
using MyManual.Models.Onboarding;

using System;
using System.Collections.Generic;
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
        public string CurrentDayText => $"DAY {SelectedViewDay}";

        private ObservableCollection<OnboardingTask>? _tasks;
        public ObservableCollection<OnboardingTask> Tasks
        {
            get => _tasks ??= new ObservableCollection<OnboardingTask>();
            set => SetProperty(ref _tasks, value);
        }

        // ==================== 주차별 페이징 ====================

        private int _currentWeek;
        public int CurrentWeek
        {
            get => _currentWeek;
            set
            {
                if (SetProperty(ref _currentWeek, value))
                {
                    OnPropertyChanged(nameof(WeekText));
                    OnPropertyChanged(nameof(CanGoPreviousWeek));
                    OnPropertyChanged(nameof(CanGoNextWeek));
                    UpdateWeekDays();
                }
            }
        }

        // 현재 주차 텍스트 (예: "Week 1 (DAY 1~5)")
        public string WeekText => $"Week {CurrentWeek} (DAY {WeekStartDay}~{WeekEndDay})";

        // 현재 주차의 시작/끝 Day
        public int WeekStartDay => (CurrentWeek - 1) * 5 + 1;
        public int WeekEndDay => CurrentWeek * 5;

        // 현재 주차에 표시할 Day 목록 (5개)
        private ObservableCollection<DayButtonInfo> _weekDays = new();
        public ObservableCollection<DayButtonInfo> WeekDays
        {
            get => _weekDays;
            set => SetProperty(ref _weekDays, value);
        }

        // 이전/다음 주 이동 가능 여부
        public bool CanGoPreviousWeek => CurrentWeek > 1;
        public bool CanGoNextWeek => WeekEndDay < CurrentDay + 5; // 미래 1주까지만 허용

        // ==================== Commands ====================

        public ICommand ToggleTaskCommand { get; }  // 체크박스 토글
        public ICommand OpenManualCommand { get; }  // 매뉴얼 열기
        public ICommand NextWeekCommand { get; }    // 다음 주
        public ICommand PreviousWeekCommand { get; } // 이전 주
        public ICommand SelectDayCommand { get; }   // Day 버튼 클릭

        // ==================== 생성자 ====================

        public OnboardingViewModel(User user)
        {
            // 현재 사용자 로드 (임시 데이터)
            CurrentUser = user;

            // 현재 Day 계산 (입사일부터 오늘까지 근무일 수)
            CurrentDay = WorkingDayCalculator.GetWorkingDay(CurrentUser.JoinDate);

            // 현재 Day가 속한 주차 계산 (1~5: Week1, 6~10: Week2, ...)
            _currentWeek = (CurrentDay - 1) / 5 + 1;

            // 현재 보고 있는 Day 초기화 (오늘 Day로 시작)
            _selectedViewDay = CurrentDay;

            // 주차별 Day 버튼 초기화
            UpdateWeekDays();

            // 작업 로드
            LoadTasksForDay(_selectedViewDay);

            // Command 초기화
            ToggleTaskCommand = new RelayCommand(OnToggleTask);
            OpenManualCommand = new RelayCommand(OnOpenManual, CanOpenManual);
            NextWeekCommand = new RelayCommand(OnNextWeek, _ => CanGoNextWeek);
            PreviousWeekCommand = new RelayCommand(OnPreviousWeek, _ => CanGoPreviousWeek);
            SelectDayCommand = new RelayCommand(OnSelectDay, CanSelectDay);
        }

        // ==================== 메서드 ====================

        private void UpdateCounts()
        {
            CompletedCount = Tasks.Count(t => t.IsCompleted);
            TotalCount = Tasks.Count;
        }

        // 체크박스 토글 - TwoWay 바인딩으로 이미 IsCompleted가 변경되므로 Count만 업데이트
        private void OnToggleTask(object? parameter)
        {
            // IsChecked TwoWay 바인딩으로 이미 값이 변경됨
            // 여기서는 Count 업데이트만 수행
            UpdateCounts();

            // 모두 완료 시
            if (CompletedCount == TotalCount && TotalCount > 0)
            {
                // TODO: 축하 메시지 또는 자동으로 다음 Day
                // MessageBox.Show("오늘 할 일을 모두 완료했습니다! 👏");
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

        // 다음 주로 이동
        private void OnNextWeek(object? parameter)
        {
            CurrentWeek++;
        }

        // 이전 주로 이동
        private void OnPreviousWeek(object? parameter)
        {
            CurrentWeek--;
        }

        // 주차별 Day 버튼 목록 업데이트
        private void UpdateWeekDays()
        {
            WeekDays.Clear();
            for (int i = 0; i < 5; i++)
            {
                int day = WeekStartDay + i;
                WeekDays.Add(new DayButtonInfo
                {
                    Day = day,
                    DisplayText = $"DAY {day}",
                    IsEnabled = day <= CurrentDay,  // 현재 근무일까지만 활성화
                    IsCurrentDay = day == CurrentDay
                });
            }
        }

        // Day 버튼 클릭
        private void OnSelectDay(object? parameter)
        {
            int selectedDay = 0;

            // string 또는 int 둘 다 처리
            if (parameter is string dayStr && int.TryParse(dayStr, out int parsed))
            {
                selectedDay = parsed;
            }
            else if (parameter is int dayInt)
            {
                selectedDay = dayInt;
            }

            if (selectedDay > 0 && selectedDay <= CurrentDay)
            {
                _selectedViewDay = selectedDay;
                OnPropertyChanged(nameof(SelectedViewDay));
                LoadTasksForDay(selectedDay);
            }
        }

        private bool CanSelectDay(object? parameter)
        {
            int selectedDay = 0;

            if (parameter is string dayStr && int.TryParse(dayStr, out int parsed))
            {
                selectedDay = parsed;
            }
            else if (parameter is int dayInt)
            {
                selectedDay = dayInt;
            }

            // 현재 근무일까지만 선택 가능
            return selectedDay > 0 && selectedDay <= CurrentDay;
        }

        // 현재 보고 있는 Day (UI 표시용)
        private int _selectedViewDay;
        public int SelectedViewDay
        {
            get => _selectedViewDay;
            set => SetProperty(ref _selectedViewDay, value);
        }

        // 특정 Day의 작업 로드
        private void LoadTasksForDay(int day)
        {
            Tasks.Clear();

            // Day별 작업 정의 (더미 데이터)
            var tasksByDay = GetTasksForDay(day);
            foreach (var task in tasksByDay)
            {
                Tasks.Add(task);
            }

            UpdateCounts();
            OnPropertyChanged(nameof(CurrentDayText));
        }

        private List<OnboardingTask> GetTasksForDay(int day)
        {
            return day switch
            {
                1 => new List<OnboardingTask>
                {
                    new() { Id = 1, Title = "사내 메일 할당받기", ManualId = 101 },
                    new() { Id = 2, Title = "PC 및 업무 장비 수령", ManualId = 102 },
                    new() { Id = 3, Title = "사내 시스템 계정 발급", ManualId = 103 },
                    new() { Id = 4, Title = "팀원 소개 받기", ManualId = 104 },
                    new() { Id = 5, Title = "회사 규정 숙지", ManualId = 105 }
                },
                2 => new List<OnboardingTask>
                {
                    new() { Id = 6, Title = "프로젝트 현황 파악", ManualId = 201 },
                    new() { Id = 7, Title = "개발 환경 세팅", ManualId = 202 },
                    new() { Id = 8, Title = "코드 저장소 접근 권한 받기", ManualId = 203 },
                    new() { Id = 9, Title = "팀 위키/문서 확인", ManualId = 204 }
                },
                3 => new List<OnboardingTask>
                {
                    new() { Id = 10, Title = "첫 번째 태스크 할당받기", ManualId = 301 },
                    new() { Id = 11, Title = "코드 리뷰 프로세스 이해", ManualId = 302 },
                    new() { Id = 12, Title = "일일 스탠드업 참여", ManualId = 303 }
                },
                4 => new List<OnboardingTask>
                {
                    new() { Id = 13, Title = "첫 번째 PR 작성", ManualId = 401 },
                    new() { Id = 14, Title = "QA 프로세스 이해", ManualId = 402 },
                    new() { Id = 15, Title = "배포 파이프라인 확인", ManualId = 403 }
                },
                5 => new List<OnboardingTask>
                {
                    new() { Id = 16, Title = "1주차 회고 작성", ManualId = 501 },
                    new() { Id = 17, Title = "멘토와 1:1 미팅", ManualId = 502 },
                    new() { Id = 18, Title = "다음 주 목표 설정", ManualId = 503 }
                },
                // Week 2 (DAY 6~10)
                6 => new List<OnboardingTask>
                {
                    new() { Id = 19, Title = "2주차 업무 시작", ManualId = 601 },
                    new() { Id = 20, Title = "담당 모듈 분석", ManualId = 602 },
                    new() { Id = 21, Title = "기술 문서 작성 시작", ManualId = 603 }
                },
                7 => new List<OnboardingTask>
                {
                    new() { Id = 22, Title = "코드 분석 및 리팩토링", ManualId = 701 },
                    new() { Id = 23, Title = "단위 테스트 작성", ManualId = 702 },
                    new() { Id = 24, Title = "버그 수정 경험", ManualId = 703 }
                },
                8 => new List<OnboardingTask>
                {
                    new() { Id = 25, Title = "기능 개발 참여", ManualId = 801 },
                    new() { Id = 26, Title = "코드 리뷰 피드백 반영", ManualId = 802 },
                    new() { Id = 27, Title = "팀 미팅 발표", ManualId = 803 },
                    new() { Id = 28, Title = "문서화 완료", ManualId = 804 }
                },
                _ => new List<OnboardingTask>
                {
                    // Day 9 이상은 자유 업무
                    new() { Id = 100 + day, Title = $"Day {day} 자율 업무", ManualId = 900 }
                }
            };
        }
    }

    // Day 버튼 정보 클래스
    public class DayButtonInfo : ViewModelBase
    {
        public int Day { get; set; }
        public string DisplayText { get; set; } = string.Empty;
        public bool IsEnabled { get; set; }
        public bool IsCurrentDay { get; set; }
    }
}