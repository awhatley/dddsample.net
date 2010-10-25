using System;

using DomainDrivenDelivery.Domain.Patterns;
using DomainDrivenDelivery.Domain.Patterns.Entity;
using DomainDrivenDelivery.Utilities;

namespace DomainDrivenDelivery.Domain.Model.Locations
{
    /// <summary>
    /// A location is our model is stops on a journey, such as cargo
    /// origin or destination, or carrier movement endpoints.
    /// </summary>
    /// <remarks>
    /// It is uniquely identified by a UN Locode.
    /// </remarks>
    public class Location : EntitySupport<Location, UnLocode>
    {
        private readonly UnLocode _unLocode;
        private readonly String _name;
        private TimeZoneInfo _timeZone;
        private CustomsZone _customsZone;

        /// <summary>
        /// Special Location object that marks an unknown location.
        /// </summary>
        public static readonly Location NONE = new Location(
            new UnLocode("XXXXX"), "-", TimeZoneInfo.FindSystemTimeZoneById("Zulu"), null
        );

        internal Location(UnLocode unLocode, String name, TimeZoneInfo timeZone, CustomsZone customsZone)
        {
            Validate.notNull(unLocode);
            Validate.notNull(name);
            Validate.notNull(timeZone);
            //    Validate.notNull(customsZone);

            this._unLocode = unLocode;
            this._name = name;
            this._timeZone = timeZone;
            this._customsZone = customsZone;
        }

        public override UnLocode identity()
        {
            return _unLocode;
        }

        /// <summary>
        /// UN Locode for this location.
        /// </summary>
        /// <returns>UN Locode for this location.</returns>
        public UnLocode unLocode()
        {
            return _unLocode;
        }

        /// <summary>
        /// Actual name of this location, e.g. "Stockholm".
        /// </summary>
        /// <returns>Actual name of this location, e.g. "Stockholm".</returns>
        public String name()
        {
            return _name;
        }

        /// <summary>
        /// Customs zone of this location.
        /// </summary>
        /// <returns>Customs zone of this location.</returns>
        public CustomsZone customsZone()
        {
            return _customsZone;
        }

        /// <summary>
        /// Time zone of this location.
        /// </summary>
        /// <returns>Time zone of this location.</returns>
        public TimeZoneInfo timeZone()
        {
            return _timeZone;
        }

        public override string ToString()
        {
            return _name + " [" + _unLocode + "]";
        }

        Location()
        {
            // Needed by Hibernate
            _unLocode = null;
            _name = null;
        }
    }
}