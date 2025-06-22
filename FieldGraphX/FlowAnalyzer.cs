using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System.Collections.Generic;
using FieldGraphX.Models;
using Newtonsoft.Json.Linq;
using System.Web.UI.WebControls;
using XrmToolBox.Extensibility;
using System.Linq;

namespace FlowGraphX
{
    public class FlowAnalyzer : PluginControlBase
    {
        private readonly IOrganizationService _service;

        public FlowAnalyzer(IOrganizationService service)
        {
            _service = service;
        }

        public List<FlowUsage> AnalyzeFlows(string entityLogicalName, string fieldLogicalName)
        {
            var results = new List<FlowUsage>();
            var query = new QueryExpression("workflow")
            {
                ColumnSet = new ColumnSet("name", "clientdata"),
                Criteria =
                {
                    Conditions =
                    {
                        new ConditionExpression("category", ConditionOperator.Equal, 5),
                        new ConditionExpression("type", ConditionOperator.Equal, 1)
                    }
                }
            };

            var flows = _service.RetrieveMultiple(query).Entities;
            foreach (var flow in flows)
            {
                string name = flow.GetAttributeValue<string>("name");
                string json = flow.GetAttributeValue<string>("clientdata");
                if (string.IsNullOrEmpty(json)) continue;

                bool containsEntity = json.ToLower().Contains(entityLogicalName.ToLower());
                bool containsField = json.ToLower().Contains(fieldLogicalName.ToLower());

                if (containsEntity && containsField)
                {
                    LogInfo($"Flow '{name}' uses field '{fieldLogicalName}' in entity '{entityLogicalName}' Json {json}.");
                    var usage = new FlowUsage
                    {
                        FlowName = name,
                        TriggerType = ExtractTriggerType(json),
                        IsFieldUsedAsTrigger = json.ToLower().Contains($"\"{fieldLogicalName}\"") &&
                                               json.ToLower().Contains("trigger"),
                        IsFieldSet = json.ToLower().Contains($"\"{fieldLogicalName}\"") &&
                                     json.ToLower().Contains("set")
                    };
                    results.Add(usage);
                }
            }

            return results;
        }

        private string ExtractTriggerType(string json)
        {
            try
            {
                JObject j = JObject.Parse(json);
                var definition = j["definition"];
                var triggers = definition?["triggers"] as JObject;
                if (triggers != null && triggers.Properties().Any())
                {
                    var triggerProperty = triggers.Properties().First();
                    return triggerProperty.Name;
                }
            }
            catch { }
            return "Unbekannt";
        }
    }
}
