using System.Security.Principal;

namespace Improving.MediatR.Tests
{
    public class PingHistory : DTO
    {
        public IIdentity Identity { get; set; }
    }
}
