namespace MyManual.Services.Interfaces
{
    public interface INavigationService
    {
        void NavigateToUserInit();
        void NavigateToOnboarding();
        void NavigateToCategoryMenu();
        void NavigateToManual();
        void NavigateToManualDetail(int manualId);
        void NavigateToManualByCategory(string category);
        void NavigateToManualCreate();
        void NavigateToManualEdit(int manualId);
        void NavigateToOnboardingManage();
    }
}
