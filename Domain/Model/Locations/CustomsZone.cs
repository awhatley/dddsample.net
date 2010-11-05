using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using DomainDrivenDelivery.Domain.Patterns.ValueObject;
using DomainDrivenDelivery.Utilities;

namespace DomainDrivenDelivery.Domain.Model.Locations
{
    /// <summary>
    /// A geographical zone within which there are no customs restrictions or checks.
    /// </summary>
    public class CustomsZone : ValueObjectSupport<CustomsZone>
    {
        // TODO: Find out what the standards are for this, if any. For now:
        // For CustomsZone code, we are using the "country code" portion of the UnLocode,
        // except within economic areas that are not countries, such as the EU. Then we make one up.
        // Country code is exactly two letters, so we'll use 2 letters.
        private static readonly Regex ValidPattern = new Regex("[a-zA-Z]{2}", RegexOptions.Compiled);

        /// <summary>
        /// Null object pattern.
        /// </summary>
        public static readonly CustomsZone None = new CustomsZone("AA", "1");

        /// <summary>
        /// Code, always upper case.
        /// </summary>
        public virtual string Code { get; private set; }

        /// <summary>
        /// The name.
        /// </summary>
        public virtual string Name { get; private set; }

        internal CustomsZone(string code, string name)
        {
            Validate.notNull(code, "Code is required");
            Validate.isTrue(ValidPattern.IsMatch(code), code + " is not a valid customs zone code (does not match pattern)");
            Validate.notNull(name, "Name is required");

            Code = code.ToUpperInvariant();
            Name = name;
        }

        /// <summary>
        /// Where would a Cargo enter this CustomsZone if it were
        /// following this route. Specific voyages, etc do not
        /// matter, only the sequence of Locations.
        /// </summary>
        /// <param name="route">a list of locations</param>
        /// <returns>The first location on the route that is in this customs zone.</returns>
        public virtual Location EntryPoint(IEnumerable<Location> route)
        {
            return route.FirstOrDefault(Includes);
        }

        /// <summary>
        /// Convenience method for testing.
        /// </summary>
        /// <param name="route">a list of locations</param>
        /// <returns>The first location on the route that is in this customs zone.</returns>
        public virtual Location EntryPoint(params Location[] route)
        {
            return EntryPoint((IEnumerable<Location>)route);
        }

        /// <summary>
        /// If the rules of the CustomsZone were different or more complex,
        /// this is where that rule would be expressed. For the example, we'll
        /// just use the basic rule -- customs clearance is at the entry point.
        /// </summary>
        /// <param name="route">a list of locations</param>
        /// <returns>The clearance point for the list of locations.</returns>
        public virtual Location ClearancePoint(IEnumerable<Location> route)
        {
            return EntryPoint(route);
        }

        /// <summary>
        /// Convenience method for testing.
        /// </summary>
        /// <param name="route">a list of locations</param>
        /// <returns>The clearance point for the list of locations.</returns>
        public virtual Location ClearancePoint(params Location[] route)
        {
            return ClearancePoint((IEnumerable<Location>)route);
        }

        public virtual bool Includes(Location location)
        {
            return sameValueAs(location.CustomsZone);
        }

        public override string ToString()
        {
            return Code + "[" + Name + "]";
        }

        protected internal CustomsZone()
        {
        }
    }
}