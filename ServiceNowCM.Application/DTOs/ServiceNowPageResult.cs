using System.Text.Json;

namespace ServiceNowCM.Application.DTOs
{
    public class ServiceNowPageResult
    {
        public IReadOnlyList<Dictionary<string, JsonElement>> Records { get; set; }
            = new List<Dictionary<string, JsonElement>>();

        public int Offset { get; set; }

        public int PageSize { get; set; }

        public int ReturnedCount { get; set; }

        public bool HasMore { get; set; }
    }
}