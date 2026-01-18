namespace MyManual.Exceptions
{
    /// <summary>
    /// 작업 결과를 나타내는 제네릭 클래스 (Result Pattern)
    /// </summary>
    public class Result<T>
    {
        public bool IsSuccess { get; }
        public T? Value { get; }
        public string? ErrorMessage { get; }
        public Exception? Exception { get; }

        private Result(bool isSuccess, T? value, string? errorMessage, Exception? exception)
        {
            IsSuccess = isSuccess;
            Value = value;
            ErrorMessage = errorMessage;
            Exception = exception;
        }

        public static Result<T> Success(T value)
        {
            return new Result<T>(true, value, null, null);
        }

        public static Result<T> Failure(string errorMessage)
        {
            return new Result<T>(false, default, errorMessage, null);
        }

        public static Result<T> Failure(Exception exception)
        {
            var message = ExceptionHandler.GetUserMessage(exception);
            return new Result<T>(false, default, message, exception);
        }

        public static Result<T> Failure(string errorMessage, Exception exception)
        {
            return new Result<T>(false, default, errorMessage, exception);
        }

        /// <summary>
        /// 성공 시 값을 변환
        /// </summary>
        public Result<TNew> Map<TNew>(Func<T, TNew> mapper)
        {
            if (IsSuccess && Value != null)
            {
                return Result<TNew>.Success(mapper(Value));
            }
            return Exception != null
                ? Result<TNew>.Failure(ErrorMessage ?? "Unknown error", Exception)
                : Result<TNew>.Failure(ErrorMessage ?? "Unknown error");
        }

        /// <summary>
        /// 성공 시 다른 Result 반환
        /// </summary>
        public Result<TNew> Bind<TNew>(Func<T, Result<TNew>> binder)
        {
            if (IsSuccess && Value != null)
            {
                return binder(Value);
            }
            return Exception != null
                ? Result<TNew>.Failure(ErrorMessage ?? "Unknown error", Exception)
                : Result<TNew>.Failure(ErrorMessage ?? "Unknown error");
        }
    }

    /// <summary>
    /// 값 없는 작업 결과
    /// </summary>
    public class Result
    {
        public bool IsSuccess { get; }
        public string? ErrorMessage { get; }
        public Exception? Exception { get; }

        private Result(bool isSuccess, string? errorMessage, Exception? exception)
        {
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
            Exception = exception;
        }

        public static Result Success()
        {
            return new Result(true, null, null);
        }

        public static Result Failure(string errorMessage)
        {
            return new Result(false, errorMessage, null);
        }

        public static Result Failure(Exception exception)
        {
            var message = ExceptionHandler.GetUserMessage(exception);
            return new Result(false, message, exception);
        }

        public static Result Failure(string errorMessage, Exception exception)
        {
            return new Result(false, errorMessage, exception);
        }
    }
}
