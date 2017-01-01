namespace Improving.MediatR.Rest.Delete
{
    using System;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    public class DeleteHandler<TDelete, TResource>
        : RestHandler<DeleteRequest<TDelete, TResource>, DeleteResponse<TResource>, TDelete, TResource>
    {
        public override async Task<DeleteResponse<TResource>> Handle(
            DeleteRequest<TDelete, TResource> deleteRequest)
        {
            var httpRequest = deleteRequest.Resource as HttpRequestMessage;
            if (httpRequest != null && httpRequest.Method != HttpMethod.Delete)
                throw new ArgumentException(
                    $"Expected HttpRequestMessage with {HttpMethod.Delete} method but got {httpRequest.Method}");

            var resourceUri = GetResourceUri(deleteRequest) ?? "";

            using (var httpClient = new HttpClient())
            {
                SetBaseAddress(httpClient, deleteRequest);

                if (Logger.IsDebugEnabled)
                {
                    Logger.DebugFormat("Http DELETE resource {0} ({1})",
                        resourceUri, httpClient.BaseAddress);
                }
               
                var response = httpRequest == null
                    ? await httpClient.DeleteAsync(resourceUri)
                    : await httpClient.SendAsync(httpRequest);

                if (Logger.IsDebugEnabled)
                {
                    var log = new StringBuilder();
                    log.AppendFormat("Http DELETE response {0}", response.StatusCode)
                        .AppendLine().Append(response);
                    Logger.Debug(log.ToString);
                }

                var resource = await ExtractResource(deleteRequest, response);
                return new DeleteResponse<TResource>(resource);
            }
        }
    }
}
