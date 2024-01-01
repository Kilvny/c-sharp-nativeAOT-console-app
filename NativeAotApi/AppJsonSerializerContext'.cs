using System.Text.Json.Serialization;

namespace NativeAotApi
{
    [JsonSerializable(typeof(Todo[]))]
    public partial class AppJsonSerializerContext : JsonSerializerContext
    {

    }
}
