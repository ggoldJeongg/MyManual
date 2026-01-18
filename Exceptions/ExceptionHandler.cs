using System.Windows;

namespace MyManual.Exceptions
{
    /// <summary>
    /// 전역 예외 처리 헬퍼
    /// </summary>
    public static class ExceptionHandler
    {
        /// <summary>
        /// 예외를 처리하고 사용자에게 메시지 표시
        /// </summary>
        public static void Handle(Exception ex, string context = "")
        {
            var message = GetUserMessage(ex, context);
            LogException(ex, context);
            ShowError(message);
        }

        /// <summary>
        /// 예외를 처리하고 사용자에게 메시지 표시 (비동기 컨텍스트용)
        /// </summary>
        public static void HandleAsync(Exception ex, string context = "")
        {
            Application.Current.Dispatcher.Invoke(() => Handle(ex, context));
        }

        /// <summary>
        /// 예외에서 사용자 친화적 메시지 추출
        /// </summary>
        public static string GetUserMessage(Exception ex, string context = "")
        {
            var prefix = string.IsNullOrEmpty(context) ? "" : $"{context}: ";

            return ex switch
            {
                AppException appEx => prefix + appEx.UserMessage,
                UnauthorizedAccessException => prefix + "권한이 없습니다.",
                KeyNotFoundException => prefix + "요청한 데이터를 찾을 수 없습니다.",
                InvalidOperationException => prefix + "잘못된 작업입니다.",
                ArgumentNullException => prefix + "필수 데이터가 누락되었습니다.",
                ArgumentException => prefix + "잘못된 데이터입니다.",
                _ => prefix + "오류가 발생했습니다. 잠시 후 다시 시도해주세요."
            };
        }

        /// <summary>
        /// 예외 로깅
        /// </summary>
        private static void LogException(Exception ex, string context)
        {
            var logMessage = string.IsNullOrEmpty(context)
                ? $"[예외] {ex.GetType().Name}: {ex.Message}"
                : $"[예외] [{context}] {ex.GetType().Name}: {ex.Message}";

            System.Diagnostics.Debug.WriteLine(logMessage);
            System.Diagnostics.Debug.WriteLine($"[StackTrace] {ex.StackTrace}");

            if (ex.InnerException != null)
            {
                System.Diagnostics.Debug.WriteLine($"[InnerException] {ex.InnerException.Message}");
            }
        }

        /// <summary>
        /// 에러 메시지 박스 표시
        /// </summary>
        private static void ShowError(string message)
        {
            MessageBox.Show(message, "오류", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        /// <summary>
        /// 경고 메시지 박스 표시
        /// </summary>
        public static void ShowWarning(string message)
        {
            MessageBox.Show(message, "경고", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        /// <summary>
        /// 정보 메시지 박스 표시
        /// </summary>
        public static void ShowInfo(string message)
        {
            MessageBox.Show(message, "알림", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// 확인 다이얼로그 표시
        /// </summary>
        public static bool Confirm(string message, string title = "확인")
        {
            var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
            return result == MessageBoxResult.Yes;
        }
    }
}
