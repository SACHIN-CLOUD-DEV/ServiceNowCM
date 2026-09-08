namespace ServiceNowCM.Api.Models
{
    public class ApiErrorResponse
    {
        public int Status { get; set; }

        public string Message { get; set; } = string.Empty;
    }
}
