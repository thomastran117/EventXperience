namespace backend.DTOs
{
    public class MessageResponse
    {
        public MessageResponse(string message)
        {
            Message = message;
        }
        public string Message { get; set; } = string.Empty;
    }
    public class ApiResponse<T>
    {
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }

        public ApiResponse(string message, T data)
        {
            Message = message;
            Data = data;
        }
    }
}
