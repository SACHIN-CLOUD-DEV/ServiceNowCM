using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceNowCM.Application.DTOs
{
    public class ContentManagerCreateRecordResult
    {
        public bool Success { get; set; }

        public long? RecordUri { get; set; }

        public string? RecordNumber { get; set; }

        public string Message { get; set; } = string.Empty;
    }
}