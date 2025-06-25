using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Services.Description;
using System.Windows.Forms;
using XrmToolBox.Extensibility;

namespace FieldGraphX
{
    public class InfoLoader : PluginControlBase
    {
        private readonly IOrganizationService _service;

        public InfoLoader(IOrganizationService service)
        {
            _service = service;
        }

        public List<string> LoadEntities()
        {
            try
            {
                var query = new RetrieveAllEntitiesRequest
                {
                    EntityFilters = EntityFilters.Entity,
                    RetrieveAsIfPublished = true
                };

                var response = (RetrieveAllEntitiesResponse)_service.Execute(query);
                var entities = response.EntityMetadata.Select(e => e.LogicalName).ToList();

                return entities;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading the entities:{ex.Message}");
                return new List<string>();
            }
        }


        public List<string> LoadFields(string entityLogicalName)
        {
            try
            {
                var query = new RetrieveEntityRequest
                {
                    LogicalName = entityLogicalName,
                    EntityFilters = EntityFilters.Attributes,
                    RetrieveAsIfPublished = true
                };

                var response = (RetrieveEntityResponse)_service.Execute(query);
                var fields = response.EntityMetadata.Attributes.Select(a => a.LogicalName).ToList();

                return fields;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading the fields: {ex.Message}");
                return new List<string>();
            }
        }

    }
}
