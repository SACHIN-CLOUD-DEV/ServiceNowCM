namespace ServiceNowCM.Application.DTOs
{
    public class ServiceNowFieldInfo
    {
        public string Name { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;

        public string DataType { get; set; } = string.Empty;

        public bool IsReference { get; set; }

        public string? ReferenceTable { get; set; }
    }
}