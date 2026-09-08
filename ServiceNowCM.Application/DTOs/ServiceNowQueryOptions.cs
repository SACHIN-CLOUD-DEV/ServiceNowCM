namespace ServiceNowCM.Application.DTOs
{
    public class ServiceNowQueryOptions
    {
        public string TableName { get; set; } = string.Empty;

        public List<string> Fields { get; set; } = new();

        public string? Query { get; set; }

        public int PageSize { get; set; } = 100;

        public int Offset { get; set; } = 0;

        public bool DisplayValues { get; set; } = true;

        public bool ExcludeReferenceLinks { get; set; } = true;
    }
}