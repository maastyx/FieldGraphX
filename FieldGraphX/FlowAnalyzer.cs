using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System.Collections.Generic;
using FieldGraphX.Models;
using Newtonsoft.Json.Linq;
using System.Web.UI.WebControls;
using XrmToolBox.Extensibility;
using System.Linq;
using System;

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
                TopCount = 5000,
                Criteria =
                {
                    FilterOperator = LogicalOperator.And,
                    Conditions =
                    {
                        new ConditionExpression("category", ConditionOperator.Equal, 5),
                        new ConditionExpression("type", ConditionOperator.Equal, 1)
                    },
                    Filters =
                    {
                        new FilterExpression
                        {
                            FilterOperator = LogicalOperator.Or,
                            Conditions =
                            {
                                new ConditionExpression("clientdata", ConditionOperator.Like, $"%{entityLogicalName}%"),
                                new ConditionExpression("clientdata", ConditionOperator.Like, $"%{fieldLogicalName}%")
                            }
                        }
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
                        IsFieldUsedAsTrigger = IsFieldUsedInTrigger(json, entityLogicalName, fieldLogicalName),
                        IsFieldSet = IsFieldSet(json, entityLogicalName, fieldLogicalName)
                    };
                    if(usage.IsFieldSet || usage.IsFieldUsedAsTrigger)
                    {
                        results.Add(usage);
                    }
                }
            }

            return results;
        }

        private bool IsFieldUsedInTrigger(string json, string entityLogicalName, string fieldLogicalName)
        {
            try
            {
                JObject j = JObject.Parse(json);
                var definition = j["properties"]["definition"];
                var triggers = definition?["triggers"] as JObject;

                if (triggers != null)
                {
                    foreach (var trigger in triggers.Properties())
                    {
                        var triggerDetails = trigger.Value as JObject;
                        var parameters = triggerDetails?["inputs"]?["parameters"] as JObject;

                        if (parameters != null)
                        {
                            var entityName = parameters["subscriptionRequest/entityname"]?.ToString();
                            var filteringAttributes = parameters["subscriptionRequest/filteringattributes"]?.ToString();

                            // Überprüfen, ob die Entity übereinstimmt
                            if (!string.IsNullOrEmpty(entityName) && entityName.Equals(entityLogicalName, StringComparison.OrdinalIgnoreCase))
                            {
                                // Überprüfen, ob das Feld in den filteringattributes verwendet wird
                                if (!string.IsNullOrEmpty(filteringAttributes) && filteringAttributes.Split(',').Any(attr => attr.Equals(fieldLogicalName, StringComparison.OrdinalIgnoreCase)))
                                {
                                    return true;
                                }
                                return true; // Wenn kein filteringattributes vorhanden ist, aber die Entity übereinstimmt
                            }
                        }
                    }
                }
            }
            catch
            {
                // Fehlerbehandlung kann hier hinzugefügt werden
            }
            return false;
        }

        private string ExtractTriggerType(string json)
        {
            try
            {
                JObject j = JObject.Parse(json);
                var definition = j["properties"]["definition"];
                var triggers = definition?["triggers"] as JObject;
                if (triggers != null && triggers.Properties().Any())
                {
                    var triggerProperty = triggers.Properties().First();
                    return triggerProperty.Name.Replace("_"," ");
                }
            }
            catch { }
            return "Unbekannt";
        }
        private bool IsFieldSet(string json, string entityLogicalName, string fieldLogicalName)
        {
            try
            {
                JObject j = JObject.Parse(json);
                var definition = j["properties"]["definition"];
                var actions = definition?["actions"] as JObject;

                if (actions != null)
                {
                    return SearchActionsRecursively(actions, entityLogicalName, fieldLogicalName);
                }
            }
            catch
            {
                // Fehlerbehandlung kann hier hinzugefügt werden
            }
            return false;
        }

        private bool SearchActionsRecursively(JObject actions, string entityLogicalName, string fieldLogicalName)
        {
            foreach (var action in actions.Properties())
            {
                var actionDetails = action.Value as JObject;

                // Prüfen, ob "parameters" vorhanden ist
                var parameters = actionDetails?["inputs"]?["parameters"] as JObject;
                if (parameters != null)
                {
                    var entityName = parameters["entityName"]?.ToString();
                    var selectFields = parameters["$select"]?.ToString();
                    var filterCondition = parameters["$filter"]?.ToString();
                    var itemField = $"item/{fieldLogicalName}";
                    var fieldValue = parameters[itemField]?.ToString();

                    if ((!string.IsNullOrEmpty(selectFields) && selectFields.Split(',').Any(field => field.Trim().Equals(fieldLogicalName, StringComparison.OrdinalIgnoreCase))) ||
                        (!string.IsNullOrEmpty(filterCondition) && filterCondition.Contains(fieldLogicalName)) ||
                        !string.IsNullOrEmpty(fieldValue))
                    {
                        return true;
                    }
                    // Überprüfen, ob die Entity übereinstimmt
                    if (!string.IsNullOrEmpty(entityName) && entityName.Equals(entityLogicalName, StringComparison.OrdinalIgnoreCase))
                    {
                        // Überprüfen, ob das Feld in der $select-Liste, der $filter-Bedingung oder als "item/Feldname" verwendet wird
                        if ((!string.IsNullOrEmpty(selectFields) && selectFields.Split(',').Any(field => field.Trim().Equals(fieldLogicalName, StringComparison.OrdinalIgnoreCase))) ||
                            (!string.IsNullOrEmpty(filterCondition) && (filterCondition.Contains(fieldLogicalName) || filterCondition.Contains(itemField))))
                        {
                            return true;
                        }
                    }
                }

                // Prüfen, ob es verschachtelte Aktionen gibt
                var nestedActions = actionDetails?["actions"] as JObject;
                if (nestedActions != null)
                {
                    if (SearchActionsRecursively(nestedActions, entityLogicalName, fieldLogicalName))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public FlowHierarchy AnalyzeFlowsHierarchically(string entityLogicalName, string fieldLogicalName)
        {
            var hierarchy = new FlowHierarchy
            {
                EntityName = entityLogicalName,
                FieldName = fieldLogicalName
            };

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
                    var usage = new FlowUsage
                    {
                        FlowName = name,
                        TriggerType = ExtractTriggerType(json),
                        IsFieldUsedAsTrigger = IsFieldUsedInTrigger(json, entityLogicalName, fieldLogicalName),
                        IsFieldSet = IsFieldSet(json, entityLogicalName, fieldLogicalName)
                    };

                    if (usage.IsFieldSet)
                    {
                        hierarchy.FlowsThatSetField.Add(usage);
                    }
                    else if (usage.IsFieldUsedAsTrigger)
                    {
                        hierarchy.FlowsThatUseFieldAsTrigger.Add(usage);
                    }
                }
            }

            return hierarchy;
        }

    }
}
