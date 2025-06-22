namespace FieldGraphX.Models
{
    public class FlowUsage
    {
        public string FlowName { get; set; }
        public string TriggerType { get; set; }
        public bool IsFieldUsedAsTrigger { get; set; }
        public bool IsFieldSet { get; set; }
    }
}
