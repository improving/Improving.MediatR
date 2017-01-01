namespace Improving.MediatR.Rest.Put
{
    using System;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    public class PutHandler<TPut, TResource>
        : RestHandler<PutRequest<TPut, TResource>, PutResponse<TResource>, TPut, TResource>
    {
        public override async Task<PutResponse<TResource>> Handle(
            PutRequest<TPut, TResource> putRequest)
        {
            var httpRequest = putRequest.Resource as HttpRequestMessage;
            if (httpRequest != null && httpRequest.Method != HttpMethod.Put)
                throw new ArgumentException(
                    $"Expected HttpRequestMessage with {HttpMethod.Put} method but got {httpRequest.Method}");

            var resourceUri = GetResourceUri(putRequest) ?? "";

            using (var httpClient = new HttpClient())
            {
                SetBaseAddress(httpClient, putRequest);

                if (Logger.IsDebugEnabled)
                {
                    Logger.DebugFormat("Http PUT resource {0} ({1})",
                        resourceUri, putRequest.BaseAddress);
                }
               
                var response = httpRequest == null
                    ? await httpClient.PutAsync(resourceUri, GetContent(putRequest))
                    : await httpClient.SendAsync(httpRequest);

                if (Logger.IsDebugEnabled)
                {
                    var log = new StringBuilder();
                    log.AppendFormat("Http PUT response {0}", response.StatusCode)
                        .AppendLine().Append(response);
                    Logger.Debug(log.ToString);
                }

                var resource    = await ExtractResource(putRequest, response);
                var putResponse = new PutResponse<TResource>(resource);
                if (response.Headers.Location != null)
                    putResponse.ResourceUri = response.Headers.Location.AbsoluteUri;
                return putResponse;
            }
        }
    }
}
