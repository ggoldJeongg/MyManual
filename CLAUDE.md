# MyManual 프로젝트

## 프로젝트 개요
WPF 기반 온보딩 매뉴얼 앱. 신입사원이 입사 후 해야 할 일을 Day별로 체크하고, 관련 매뉴얼을 조회할 수 있는 시스템.

## 기술 스택
- .NET 8 + WPF
- MVVM 패턴
- JSON 데이터 (Data 폴더)

## 프로젝트 구조
```
OnboardingManual/
├── Models/           # 데이터 모델
│   ├── OnboardingTask.cs
│   ├── Manual.cs
│   └── User.cs
├── ViewModels/       # 뷰모델 (로직)
│   ├── Base/ViewModelBase.cs
│   ├── OnboardingViewModel.cs
│   └── ManualViewModel.cs
├── Views/            # 화면 (XAML)
│   ├── OnboardingView.xaml
│   └── ManualView.xaml
├── Services/         # 서비스
│   └── DataService.cs (JSON 로드)
├── Converters/       # 값 변환기
├── Commands/         # RelayCommand
├── Helpers/          # 유틸리티
└── Data/             # JSON 데이터
    ├── onboarding_tasks.json
    └── manuals.json
```

## Git 브랜치 전략 (Git Flow)
```
main (프로덕션)
  ↑
develop (개발 통합)
  ↑
feature/xxx (기능 개발)
```

### 현재 브랜치 상태
- `main`: 초기 커밋
- `develop`: 온보딩 기능 머지됨
- `feature/onboarding-manual`: 온보딩 화면 완료, develop에 머지됨
- `feature/manual-search`: 매뉴얼 화면 작업 중 (커밋 전)

## 현재 작업 상태
### 완료
- 온보딩 화면 (View + ViewModel)
- Day별 태스크 표시, 주차 페이징
- 체크박스 토글, 진행률 표시

### 작업 중 (feature/manual-search)
- ManualView, ManualViewModel 생성됨
- 카테고리 필터, 검색, 체크리스트 기능
- **아직 커밋 안 됨**

### 다음 작업
- 매뉴얼 화면 커밋
- 온보딩 ↔ 매뉴얼 네비게이션 연결
- 테스트

## 커밋 컨벤션
- `feat:` 새 기능
- `fix:` 버그 수정
- `refactor:` 리팩토링

## 학습 포인트
C# 초보자가 MVVM 패턴을 배우면서 만드는 프로젝트.
- ViewModelBase.cs → INotifyPropertyChanged 이해
- 데이터 바인딩 (`{Binding Property}`)
- Command 패턴 (RelayCommand)
