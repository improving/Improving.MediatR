using System;

namespace Improving.MediatR.Tests
{
    public class Ping : DTO, Request.WithResponse<Pong>
    {
        public Ping()
        {
            Timestamp = DateTime.Now;
        }
        
        public int?      DelayMs        { get; set; }

        public DateTime? Timestamp      { get; set; }

        public Exception ThrowException { get; set; }
    }

    public class Pong : DTO
    {
        public int Code            { get; set; }

        public int ThreadId        { get; set; }

        public PingHistory History { get; set; }
    }
}