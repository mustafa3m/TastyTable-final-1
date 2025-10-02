using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TastyTable.Api.Swagger
{
    /// Ensures string properties don’t show "string" as default/example.
    public class EmptyStringSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (schema == null) return;

            if (schema.Type == "string")
            {
                schema.Default = new OpenApiString(string.Empty);
                schema.Example = new OpenApiString(string.Empty);
            }

            // Recurse into object properties
            if (schema.Properties != null)
            {
                foreach (var prop in schema.Properties.Values)
                {
                    if (prop.Type == "string")
                    {
                        prop.Default = new OpenApiString(string.Empty);
                        prop.Example = new OpenApiString(string.Empty);
                    }
                }
            }

            // Handle array of strings
            if (schema.Type == "array" && schema.Items?.Type == "string")
            {
                schema.Items.Default = new OpenApiString(string.Empty);
                schema.Items.Example = new OpenApiString(string.Empty);
            }
        }
    }
}
