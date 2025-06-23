using System.Collections.Generic;

namespace FieldGraphX.Models
{
    public class FlowUsage
    {
        public string FlowName { get; set; }
        public string TriggerType { get; set; }
        public bool IsFieldUsedAsTrigger { get; set; }
        public bool IsFieldSet { get; set; }
        public string FlowUrl { get; set; }

    }

    public class FlowHierarchy
    {
        public string EntityName { get; set; }
        public string FieldName { get; set; }
        public List<FlowUsage> FlowsThatSetField { get; set; } = new List<FlowUsage>();
        public List<FlowUsage> FlowsThatUseFieldAsTrigger { get; set; } = new List<FlowUsage>();
    }
}
