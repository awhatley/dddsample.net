using DomainDrivenDelivery.Domain.Patterns.ValueObject;
using DomainDrivenDelivery.Utilities;

namespace DomainDrivenDelivery.Domain.Model.Travel
{
    /// <summary>
    /// Identifies a voyage.
    /// </summary>
    public class VoyageNumber : ValueObjectSupport<VoyageNumber>
    {
        public string Value { get; private set; }

        public VoyageNumber(string number)
        {
            Validate.notNull(number);
            Value = number;
        }

        public override string ToString()
        {
            return Value;
        }

        protected internal VoyageNumber()
        {
        }
    }
}