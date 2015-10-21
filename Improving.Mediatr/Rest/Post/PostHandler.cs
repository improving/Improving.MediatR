namespace Improving.MediatR.Rest.Post
{
    using System;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    public class PostHandler<TPost, TResource>
        : RestHandler<PostRequest<TPost, TResource>, PostResponse<TResource>, TPost, TResource>
        where TResource : class
    {
        public async override Task<PostResponse<TResource>> Handle(
            PostRequest<TPost, TResource> postRequest)
        {
            var httpRequest = postRequest.Resource as HttpRequestMessage;
            if (httpRequest != null && httpRequest.Method != HttpMethod.Post)
                throw new ArgumentException(
                    $"Expected HttpRequestMessage with {HttpMethod.Post} method but got {httpRequest.Method}");

            var resourceUri = GetResourceUri(postRequest) ?? "";

            using (var httpClient = new HttpClient())
            {
                SetBaseAddress(httpClient, postRequest);

                if (Logger.IsDebugEnabled)
                {
                    Logger.DebugFormat("Http POST resource {0} ({1})",
                        resourceUri, httpClient.BaseAddress);
                }

                var response = httpRequest == null
                    ? await httpClient.PostAsync(resourceUri, GetContent(postRequest))
                    : await httpClient.SendAsync(httpRequest);

                if (Logger.IsDebugEnabled)
                {
                    var log = new StringBuilder();
                    log.AppendFormat("Http POST response {0}", response.StatusCode)
                        .AppendLine().Append(response);
                    Logger.Debug(log.ToString);
                }

                var resource     = await ExtractResource(response);
                var postResponse = new PostResponse<TResource>(resource);
                if (response.Headers.Location != null)
                    postResponse.ResourceUri = response.Headers.Location.AbsoluteUri;
                return postResponse;
            }
        }
    }
}
