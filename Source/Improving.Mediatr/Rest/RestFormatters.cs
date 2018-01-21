namespace Improving.MediatR.Rest
{
    using System.Net.Http.Formatting;
    using System.Runtime.Serialization.Formatters;
    using Newtonsoft.Json;

    public static class RestFormatters
    {
        public static readonly JsonMediaTypeFormatter Json = new JsonMediaTypeFormatter();

        public static readonly JsonMediaTypeFormatter[] JsonList = { Json };

        public static readonly JsonMediaTypeFormatter JsonTyped = new JsonMediaTypeFormatter
        {
            SerializerSettings =
            {
                TypeNameHandling       = TypeNameHandling.Auto,
                TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple
            }
        };

        public static readonly JsonMediaTypeFormatter[] TypedJsonList = { JsonTyped };

        static RestFormatters()
        {
            Json.SerializerSettings.NullValueHandling      = NullValueHandling.Ignore;;
            JsonTyped.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
        }
    }
}
