using System.Text.RegularExpressions;

using DomainDrivenDelivery.Domain.Patterns;
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
    public sealed class UnLocode : ValueObjectSupport<UnLocode>
    {
        private readonly string _unLocode;

        // Country code is exactly two letters.
        // Location code is usually three letters, but may contain the numbers 2-9 as well
        private static readonly Regex VALID_PATTERN = new Regex("[a-zA-Z]{2}[a-zA-Z2-9]{3}", RegexOptions.Compiled);

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="countryAndLocation">Location string.</param>
        public UnLocode(string countryAndLocation)
        {
            Validate.notNull(countryAndLocation, "Country and location may not be null");
            Validate.isTrue(VALID_PATTERN.IsMatch(countryAndLocation),
              countryAndLocation + " is not a valid UN/LOCODE (does not match pattern)");

            _unLocode = countryAndLocation.ToUpperInvariant();
        }

        /// <summary>
        /// country code and location code concatenated, always upper case.
        /// </summary>
        /// <returns>country code and location code concatenated, always upper case.</returns>
        public string stringValue()
        {
            return _unLocode;
        }

        public override string ToString()
        {
            return stringValue();
        }

        UnLocode()
        {
            // Needed by Hibernate
            _unLocode = null;
        }
    }
}