namespace MyManual.Exceptions
{
    /// <summary>
    /// 애플리케이션 기본 예외 클래스
    /// </summary>
    public class AppException : Exception
    {
        public string UserMessage { get; }

        public AppException(string message) : base(message)
        {
            UserMessage = message;
        }

        public AppException(string message, string userMessage) : base(message)
        {
            UserMessage = userMessage;
        }

        public AppException(string message, Exception innerException) : base(message, innerException)
        {
            UserMessage = message;
        }
    }

    /// <summary>
    /// 데이터베이스 관련 예외
    /// </summary>
    public class DatabaseException : AppException
    {
        public DatabaseException(string message) : base(message, "데이터 처리 중 오류가 발생했습니다.") { }
        public DatabaseException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// 엔티티를 찾을 수 없는 경우
    /// </summary>
    public class EntityNotFoundException : AppException
    {
        public string EntityType { get; }
        public object? EntityId { get; }

        public EntityNotFoundException(string entityType, object? id = null)
            : base($"{entityType}을(를) 찾을 수 없습니다." + (id != null ? $" (ID: {id})" : ""))
        {
            EntityType = entityType;
            EntityId = id;
        }
    }

    /// <summary>
    /// 권한 없음 예외
    /// </summary>
    public class UnauthorizedException : AppException
    {
        public UnauthorizedException(string message = "권한이 없습니다.")
            : base(message) { }
    }

    /// <summary>
    /// 유효성 검사 실패 예외
    /// </summary>
    public class ValidationException : AppException
    {
        public string FieldName { get; }

        public ValidationException(string fieldName, string message)
            : base($"{fieldName}: {message}")
        {
            FieldName = fieldName;
        }
    }

    /// <summary>
    /// 비즈니스 로직 예외
    /// </summary>
    public class BusinessException : AppException
    {
        public BusinessException(string message) : base(message) { }
    }
}
