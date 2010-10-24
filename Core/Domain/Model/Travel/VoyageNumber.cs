using DomainDrivenDelivery.Domain.Patterns;
using DomainDrivenDelivery.Domain.Patterns.ValueObject;

namespace DomainDrivenDelivery.Domain.Model.Travel
{
    /// <summary>
    /// Identifies a voyage.
    /// </summary>
    public class VoyageNumber : ValueObjectSupport<VoyageNumber>
    {
        private readonly string _number;

        public VoyageNumber(string number)
        {
            Validate.notNull(number);

            this._number = number;
        }

        public override string ToString()
        {
            return _number;
        }

        public string stringValue()
        {
            return _number;
        }

        VoyageNumber()
        {
            // Needed by Hibernate
            _number = null;
        }
    }
}