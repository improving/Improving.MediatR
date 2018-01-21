using System;
using FluentValidation;

namespace Improving.MediatR.Tests
{
    public class PingValidator : AbstractValidator<Ping>
    {
        public PingValidator()
        {
            RuleFor(p => p.Timestamp).NotNull();
            RuleFor(p => p.Timestamp)
                .Must(p => Dates != null);
        }

        public DateTime[] Dates { get; set; }
    }
}
