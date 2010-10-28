using System;

using DomainDrivenDelivery.Domain.Model.Freight;
using DomainDrivenDelivery.Domain.Model.Locations;
using DomainDrivenDelivery.Domain.Model.Shared;
using DomainDrivenDelivery.Domain.Model.Travel;

namespace DomainDrivenDelivery.Domain.Model.Handling
{
    /// <summary>
    /// Creates handling events.
    /// </summary>
    public class HandlingEventFactory
    {
        private readonly CargoRepository cargoRepository;
        private readonly VoyageRepository voyageRepository;
        private readonly LocationRepository locationRepository;

        public HandlingEventFactory(CargoRepository cargoRepository,
                                    VoyageRepository voyageRepository,
                                    LocationRepository locationRepository)
        {
            this.cargoRepository = cargoRepository;
            this.voyageRepository = voyageRepository;
            this.locationRepository = locationRepository;
        }

        /// <summary>
        /// Creates a handling event.
        /// </summary>
        /// <param name="completionTime">when the event was completed, for example finished loading</param>
        /// <param name="trackingId">cargo tracking id</param>
        /// <param name="voyageNumber">voyage number</param>
        /// <param name="unlocode">United Nations Location Code for the location of the event</param>
        /// <param name="type">type of event</param>
        /// <param name="operatorCode">operator code</param>
        /// <returns>A handling event.</returns>
        /// <exception cref="UnknownVoyageException">if there's no voyage with this number</exception>
        /// <exception cref="UnknownCargoException">if there's no cargo with this tracking id</exception>
        /// <exception cref="UnknownLocationException">if there's no location with this UN Locode</exception>
        public HandlingEvent createHandlingEvent(DateTime completionTime, TrackingId trackingId,
                                                 VoyageNumber voyageNumber, UnLocode unlocode,
                                                 HandlingActivityType type, OperatorCode operatorCode)
        {
            var cargo = findCargo(trackingId);
            var voyage = findVoyage(voyageNumber);
            var location = findLocation(unlocode);

            try
            {
                var registrationTime = DateTime.Now;
                if(voyage == null)
                {
                    return new HandlingEvent(cargo, completionTime, registrationTime, type, location);
                }
                else
                {
                    return new HandlingEvent(cargo, completionTime, registrationTime, type, location, voyage, operatorCode);
                }
            }
            catch(Exception e)
            {
                throw new CannotCreateHandlingEventException(e.Message, e);
            }
        }

        private Cargo findCargo(TrackingId trackingId)
        {
            var cargo = cargoRepository.find(trackingId);
            if(cargo == null) throw new UnknownCargoException(trackingId);
            return cargo;
        }

        private Voyage findVoyage(VoyageNumber voyageNumber)
        {
            if(voyageNumber == null)
            {
                return null;
            }

            var voyage = voyageRepository.find(voyageNumber);
            if(voyage == null)
            {
                throw new UnknownVoyageException(voyageNumber);
            }

            return voyage;
        }

        private Location findLocation(UnLocode unlocode)
        {
            var location = locationRepository.find(unlocode);
            if(location == null)
            {
                throw new UnknownLocationException(unlocode);
            }

            return location;
        }
    }
}