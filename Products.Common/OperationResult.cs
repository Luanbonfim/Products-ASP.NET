namespace Products.Common
{
    public class OperationResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }

        public List<string> Errors { get; set; } = new List<string>();

        public OperationResult()
        {
            IsSuccess = true; 
        }

        public OperationResult(string message)
        {
            IsSuccess = true;
            Message = message;
        }

        public OperationResult(string message, bool isSuccess)
        {
            Message = message;
            IsSuccess = isSuccess;
        }

        public OperationResult(IEnumerable<string> errors)
        {
            IsSuccess = false;
            Errors.AddRange(errors);
        }
    }
}
