namespace ServiceNowCM.Application.DTOs
{
    public class ContentManagerFieldDto
    {
        public string Name { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;

        public string FieldType { get; set; } = string.Empty;

        public string DataType { get; set; } = string.Empty;

        public long? Uri { get; set; }
    }
}