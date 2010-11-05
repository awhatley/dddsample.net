using DomainDrivenDelivery.Domain.Patterns.ValueObject;
using DomainDrivenDelivery.Utilities;

namespace DomainDrivenDelivery.Domain.Model.Freight
{
    public class TrackingId : ValueObjectSupport<TrackingId>
    {
        /// <summary>
        /// String representation of this tracking id.
        /// </summary>
        public virtual string Value { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="id">Id string.</param>
        public TrackingId(string id)
        {
            Validate.notNull(id);
            Value = id;
        }

        public TrackingId(long sequenceValue)
        {
            Validate.isTrue(sequenceValue > 0, "Sequence value must be larger than 0");
            Value = "C" + sequenceValue.ToString().PadLeft(8, '0');
        }

        public override string ToString()
        {
            return Value;
        }

        protected internal TrackingId()
        {
        }
    }
}