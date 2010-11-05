using System;
using System.Threading;

using DomainDrivenDelivery.Domain.Patterns.ValueObject;

namespace DomainDrivenDelivery.Domain.Model.Handling
{
    public class EventSequenceNumber : ValueObjectSupport<EventSequenceNumber>
    {
        private static long _sequence = DateTime.Now.Millisecond;

        private long Value { get; set; }

        private EventSequenceNumber(long value)
        {
            Value = value;
        }

        public static EventSequenceNumber Next()
        {
            return new EventSequenceNumber(Interlocked.Increment(ref _sequence));
        }

        public static EventSequenceNumber ValueOf(long value)
        {
            return new EventSequenceNumber(value);
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        protected internal EventSequenceNumber()
        {
            Value = -1L;
        }
    }
}