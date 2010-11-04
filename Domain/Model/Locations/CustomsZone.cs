using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using DomainDrivenDelivery.Domain.Patterns;
using DomainDrivenDelivery.Domain.Patterns.ValueObject;
using DomainDrivenDelivery.Utilities;

namespace DomainDrivenDelivery.Domain.Model.Locations
{
    /// <summary>
    /// A geographical zone within which there are no customs restrictions or checks.
    /// </summary>
    public class CustomsZone : ValueObjectSupport<CustomsZone>
    {
        private readonly string _code;
        private readonly string _name;

        // TODO: Find out what the standards are for this, if any. For now:
        // For CustomsZone code, we are using the "country code" portion of the UnLocode,
        // except within economic areas that are not countries, such as the EU. Then we make one up.
        // Country code is exactly two letters, so we'll use 2 letters.
        private static readonly Regex VALID_PATTERN = new Regex("[a-zA-Z]{2}", RegexOptions.Compiled);

        internal CustomsZone(string code, string name)
        {
            Validate.notNull(code, "Code is required");
            Validate.isTrue(VALID_PATTERN.IsMatch(code),
                            code + " is not a valid customs zone code (does not match pattern)");
            Validate.notNull(name, "Name is required");

            this._code = code.ToUpperInvariant();
            this._name = name;
        }

        /// <summary>
        /// Where would a Cargo enter this CustomsZone if it were
        /// following this route. Specific voyages, etc do not
        /// matter, only the sequence of Locations.
        /// </summary>
        /// <param name="route">a list of locations</param>
        /// <returns>The first location on the route that is in this customs zone.</returns>
        public Location entryPoint(IEnumerable<Location> route)
        {
            foreach(Location location in route)
            {
                if(this.includes(location))
                {
                    return location;
                }
            }
            return null; //The route does not enter this CustomsZone
        }

        /// <summary>
        /// Convenience method for testing. Usage in application would be
        /// <see cref="entryPoint(IEnumerable{Location})"/>
        /// </summary>
        /// <param name="route">a list of locations</param>
        /// <returns>The first location on the route that is in this customs zone.</returns>
        public Location entryPoint(params Location[] route)
        {
            return entryPoint((IEnumerable<Location>)route);
        }

        /// <summary>
        /// If the rules of the CustomsZone were different or more complex,
        /// this is where that rule would be expressed. For the example, we'll
        /// just use the basic rule -- customs clearance is at the entry point.
        /// </summary>
        /// <param name="route">a list of locations</param>
        /// <returns>The clearance point for the list of locations.</returns>
        public Location clearancePoint(IEnumerable<Location> route)
        {
            return entryPoint(route);
        }

        /// <summary>
        /// Convenience method for testing. Usage in application would be
        /// <see cref="clearancePoint(IEnumerable{Location})"/>
        /// </summary>
        /// <param name="route">a list of locations</param>
        /// <returns>The clearance point for the list of locations.</returns>
        public Location clearancePoint(params Location[] route)
        {
            return clearancePoint((IEnumerable<Location>)route);
        }

        /// <summary>
        /// Code, always upper case.
        /// </summary>
        /// <returns>Code, always upper case.</returns>
        public String code()
        {
            return _code;
        }

        /// <summary>
        /// The name.
        /// </summary>
        /// <returns>The name.</returns>
        public String name()
        {
            return _name;
        }

        internal bool includes(Location location)
        {
            return this.sameValueAs(location.CustomsZone);
        }

        public override string ToString()
        {
            return _code + "[" + _name + "]";
        }

        CustomsZone()
        {
            // Needed by Hibernate
            _code = _name = null;
        }
    }
}