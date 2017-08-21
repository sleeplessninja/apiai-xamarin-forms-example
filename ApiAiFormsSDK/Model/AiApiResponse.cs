using System;
using System.Collections.Generic;

namespace ApiAiSDK.Model
{
    public class AiApiResponse
    {
        public string Action { get; set; }
        public bool ActionIncomplete { get; set; }
        public float Score { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
        public string ResolvedQuery { get; set; }
        public int? ErrorCode { get; set; }
        public string Speech { get; set; }
    }
}
