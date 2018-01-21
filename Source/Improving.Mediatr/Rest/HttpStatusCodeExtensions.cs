using System.Net;

namespace Improving.MediatR.Rest
{
    public class HttpStatusCodeExtensions
    {
        public const int UnprocessableEntityCode = 422;

        public static HttpStatusCodeExtensions UnprocessableEntity 
            = new HttpStatusCodeExtensions(UnprocessableEntityCode);

        private HttpStatusCodeExtensions(int code)
        {
            Code = code;
        }

        public int Code { get; }

        public static implicit operator HttpStatusCode(HttpStatusCodeExtensions extension)
        {
            return (HttpStatusCode)extension.Code;
        }
    }
}
