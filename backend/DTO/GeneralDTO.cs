namespace backend.DTOs
{
    public class MessageResponse
    {
        public MessageResponse(string message, bool success = true, int? statusCode = null)
        {
            Message = message;
            Success = success;
            StatusCode = statusCode;
        }
        public string Message { get; set; } = string.Empty;
        public bool Success { get; set; }
        public int? StatusCode { get; set; }
    }
}