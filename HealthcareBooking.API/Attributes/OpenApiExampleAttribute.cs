using System.Text.Json.Nodes;

namespace HealthcareBooking.API.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
public sealed class OpenApiExampleAttribute : Attribute
{
    public OpenApiExampleAttribute(string json)
    {
        Json = json;
    }

    public string Json { get; }

    public JsonNode? ToJsonNode()
    {
        return JsonNode.Parse(Json);
    }
}
