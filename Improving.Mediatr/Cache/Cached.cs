namespace Improving.MediatR.Cache
{
    using System;
    using System.Text;

    /// <summary>
    /// Represents a cached request.
    /// </summary>
    /// <typeparam name="TResponse">The response type</typeparam>
    public class Cached<TResponse> 
        : Request.WithResponse<TResponse>
        where TResponse : class
    {
        public Cached(Request.WithResponse<TResponse> request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            Request    = request;
            TimeToLive = TimeSpan.FromDays(1);
        }

        public Request.WithResponse<TResponse> Request { get; set; }

        public TimeSpan TimeToLive { get; set; }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine().AppendFormat("Cached {0} {{", Request.GetType())
                   .AppendLine().AppendFormat("    TimeToLive: {0:c}", TimeToLive)
                   .AppendLine().Append("}").AppendLine();
            return builder.ToString();
        }
    }
}
