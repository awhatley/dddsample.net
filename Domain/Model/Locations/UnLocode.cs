using System.Text.RegularExpressions;

using DomainDrivenDelivery.Domain.Patterns.ValueObject;
using DomainDrivenDelivery.Utilities;

namespace DomainDrivenDelivery.Domain.Model.Locations
{
    /// <summary>
    /// United nations location code.
    /// </summary>
    /// <remarks>
    /// http://www.unece.org/cefact/locode/
    /// http://www.unece.org/cefact/locode/DocColumnDescription.htm#LOCODE
    /// </remarks>
    public class UnLocode : ValueObjectSupport<UnLocode>
    {
        // Country code is exactly two letters.
        // Location code is usually three letters, but may contain the numbers 2-9 as well
        private static readonly Regex VALID_PATTERN = new Regex("^[a-zA-Z]{2}[a-zA-Z2-9]{3}$", RegexOptions.Compiled);

        /// <summary>
        /// Country code and location code concatenated, always upper case.
        /// </summary>
        public virtual string Value { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="countryAndLocation">Location string.</param>
        public UnLocode(string countryAndLocation)
        {
            Validate.notNull(countryAndLocation, "Country and location may not be null");
            Validate.isTrue(VALID_PATTERN.IsMatch(countryAndLocation),
              countryAndLocation + " is not a valid UN/LOCODE (does not match pattern)");

            Value = countryAndLocation.ToUpperInvariant();
        }

        public override string ToString()
        {
            return Value;
        }

        protected internal UnLocode()
        {
        }
    }
}