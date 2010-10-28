using System;

using DomainDrivenDelivery.Domain.Patterns.ValueObject;
using DomainDrivenDelivery.Utilities;

namespace DomainDrivenDelivery.Domain.Model.Freight
{
    [Serializable]
    public sealed class TrackingId : ValueObjectSupport<TrackingId>
    {
        private readonly string _id;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="id">Id string.</param>
        public TrackingId(string id)
        {
            Validate.notNull(id);
            this._id = id;
        }

        public TrackingId(long sequenceValue)
        {
            Validate.isTrue(sequenceValue > 0, "Sequence value must be larger than 0");
            this._id = "C" + sequenceValue.ToString().PadLeft(8, '0');
        }

        /// <summary>
        /// String representation of this tracking id.
        /// </summary>
        /// <returns>String representation of this tracking id.</returns>
        public string stringValue()
        {
            return _id;
        }

        public override string ToString()
        {
            return _id;
        }

        TrackingId()
        {
            // Needed by Hibernate
            _id = null;
        }
    }
}