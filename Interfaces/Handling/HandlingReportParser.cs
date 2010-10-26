using System;
using System.Collections.Generic;

using DomainDrivenDelivery.Domain.Model.Frieght;
using DomainDrivenDelivery.Domain.Model.Handling;
using DomainDrivenDelivery.Domain.Model.Locations;
using DomainDrivenDelivery.Domain.Model.Shared;
using DomainDrivenDelivery.Domain.Model.Travel;

namespace DomainDrivenDelivery.Interfaces.Handling
{
    /// <summary>
    /// Utility methods for parsing various forms of handling report formats.
    /// </summary>
    /// <remarks>
    /// Supports the notification pattern for incremental error reporting.
    /// </remarks>
    public static class HandlingReportParser
    {
        public static UnLocode parseUnLocode(string unlocode, List<string> errors)
        {
            try
            {
                return new UnLocode(unlocode);
            }
            catch(ArgumentException e)
            {
                errors.Add(e.Message);
                return null;
            }
        }

        public static TrackingId parseTrackingId(string trackingId, List<string> errors)
        {
            try
            {
                return new TrackingId(trackingId);
            }
            catch(ArgumentException e)
            {
                errors.Add(e.Message);
                return null;
            }
        }

        public static VoyageNumber parseVoyageNumber(string voyageNumber, List<string> errors)
        {
            if(!String.IsNullOrEmpty(voyageNumber))
            {
                try
                {
                    return new VoyageNumber(voyageNumber);
                }
                catch(ArgumentException e)
                {
                    errors.Add(e.Message);
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public static DateTime parseDate(string completionTime, List<string> errors)
        {
            DateTime date;

            if(!DateTime.TryParse(completionTime, out date))
            {
                errors.Add("Invalid date format: " + completionTime + ", must be in valid DateTime format.");
            }

            return date;
        }

        public static HandlingActivityType parseEventType(string eventType, List<string> errors)
        {
            HandlingActivityType type;

            if(!Enum.TryParse(eventType, true, out type))
            {
                errors.Add(eventType + " is not a valid handling event type. Valid types are: " +
                    Enum.GetValues(typeof(HandlingActivityType)));
            }

            return type;
        }

        public static OperatorCode parseOperatorCode()
        {
            // TODO stubbed atm
            return new OperatorCode("ABCDE");
        }
    }
}