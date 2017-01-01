namespace Improving.MediatR.Rest.Get
{
    using System;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    public class GetHandler<TGet, TResource>
        : RestHandler<GetRequest<TGet, TResource>, GetResponse<TResource>, TGet, TResource>
    {
        public override async Task<GetResponse<TResource>>
            Handle(GetRequest<TGet, TResource> getRequest) 
        {
            var httpRequest = getRequest.Resource as HttpRequestMessage;
            if (httpRequest != null && httpRequest.Method != HttpMethod.Get)
                throw new ArgumentException(
                    $"Expected HttpRequestMessage with {HttpMethod.Get} method but got {httpRequest.Method}");

            var resourceUri = GetResourceUri(getRequest) ?? "";

            using (var httpClient = new HttpClient())
            {
                SetBaseAddress(httpClient, getRequest);

                if (Logger.IsDebugEnabled)
                {
                    Logger.DebugFormat("Http GET resource {0} ({1})",
                        resourceUri, httpClient.BaseAddress);
                }
                
                var response = httpRequest == null
                    ? await httpClient.GetAsync(resourceUri)
                    : await httpClient.SendAsync(httpRequest);

                if (Logger.IsDebugEnabled)
                {
                    var log = new StringBuilder();
                    log.AppendFormat("Http GET response {0}", response.StatusCode)
                        .AppendLine().Append(response);
                    Logger.Debug(log.ToString);
                }

                var resource = await ExtractResource(getRequest, response);
                return new GetResponse<TResource>(resource);
            }
        }
    }
}
