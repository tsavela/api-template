using Newtonsoft.Json.Linq;

namespace Api.Tests.Setup
{
    public static class TestExtensions
    {
        public static string PrettifyJson(this string json)
        {
            var jt = JToken.Parse(json);
            return jt.ToString(Newtonsoft.Json.Formatting.Indented);
        }
    }
}
