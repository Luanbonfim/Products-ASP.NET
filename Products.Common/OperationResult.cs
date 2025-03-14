namespace Products.Common
{
    public class OperationResult<T>
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public List<string> Errors { get; set; } = new List<string>();

        public static OperationResult<T> Success(T data, string message = null)
        {
            return new OperationResult<T>
            {
                IsSuccess = true,
                Data = data,
                Message = message
            };
        }

        public static OperationResult<T> Failure(string error)
        {
            return new OperationResult<T>
            {
                IsSuccess = false,
                Errors = new List<string> { error }
            };
        }

        public static OperationResult<T> NotFound(string message = "Resource not found")
        {
            return new OperationResult<T>
            {
                IsSuccess = false,
                Message = message,
                Errors = new List<string> { message }
            };
        }
    }
}
