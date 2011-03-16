using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using DomainDrivenDelivery.Booking.Web.Models;
using DomainDrivenDelivery.Domain.Model.Freight;
using DomainDrivenDelivery.Domain.Model.Handling;
using DomainDrivenDelivery.Domain.Model.Shared;

using Spring.Transaction.Interceptor;

namespace DomainDrivenDelivery.Booking.Web.Controllers
{
    public class TrackingController : Controller
    {
        private readonly CargoRepository _cargoRepository;
        private readonly HandlingEventRepository _handlingEventRepository;

        public TrackingController(CargoRepository cargoRepository, HandlingEventRepository handlingEventRepository)
        {
            _cargoRepository = cargoRepository;
            _handlingEventRepository = handlingEventRepository;
        }

        public ActionResult Track()
        {
            return View();
        }

        [HttpPost]
        [Transaction]
        public ActionResult Track(string id)
        {
            var trackingId = new TrackingId(id);
            var cargo = _cargoRepository.find(trackingId);

            if(cargo == null)
            {
                ModelState.AddModelError("trackingId", "Unknown tracking id");
                return View();
            }

            var handlingEvents = _handlingEventRepository
                .lookupHandlingHistoryOfCargo(cargo)
                .distinctEventsByCompletionTime();

            var model = BuildCargoTrackingViewModel(cargo, handlingEvents);
            return View(model);
        }

        private CargoTrackingViewModel BuildCargoTrackingViewModel(Cargo cargo, IEnumerable<HandlingEvent> handlingEvents)
        {
            return new CargoTrackingViewModel {
                TrackingId = cargo.TrackingId.Value,
                StatusText = GetCargoStatusText(cargo),
                Destination = cargo.RouteSpecification.Destination.Name,
                Origin = cargo.RouteSpecification.Origin.Name,
                Eta = cargo.EstimatedTimeOfArrival.ToString("yyyy-MM-dd hh:mm"),
                NextExpectedActivity = GetCargoNextExpectedActivity(cargo),
                IsMisdirected = cargo.IsMisdirected,
                Events = handlingEvents.Select(BuildHandlingEventViewModel),
            };
        }

        private string GetCargoStatusText(Cargo cargo)
        {
            switch(cargo.TransportStatus)
            {
                case TransportStatus.IN_PORT:
                    return String.Format("In port {0}", cargo.LastKnownLocation.Name);

                case TransportStatus.ONBOARD_CARRIER:
                    return String.Format("Onboard voyage {0}", cargo.CurrentVoyage.VoyageNumber.Value);

                case TransportStatus.CLAIMED:
                    return "Claimed";

                case TransportStatus.NOT_RECEIVED:
                    return "Not received";

                default:
                    return "Unknown";
            }
        }

        private string GetCargoNextExpectedActivity(Cargo cargo)
        {
            if(cargo.NextExpectedActivity == null)
                return String.Empty;

            switch(cargo.NextExpectedActivity.Type)
            {
                case HandlingActivityType.LOAD:
                    return "Next expected activity is to load cargo onto voyage " +
                           cargo.NextExpectedActivity.Voyage.VoyageNumber + " in " +
                           cargo.NextExpectedActivity.Location.Name;

                case HandlingActivityType.UNLOAD:
                    return "Next expected activity is to unload cargo off of " +
                           cargo.NextExpectedActivity.Voyage.VoyageNumber + " in " +
                           cargo.NextExpectedActivity.Location.Name;

                default:
                    return "Next expected activity is to " + cargo.NextExpectedActivity.Type.ToString().ToLower() +
                           " cargo in " + cargo.NextExpectedActivity.Location.Name;
            }
        }

        private CargoHandlingEventViewModel BuildHandlingEventViewModel(HandlingEvent handlingEvent)
        {
            return new CargoHandlingEventViewModel {
                Location = handlingEvent.Location.Name,
                Time = handlingEvent.CompletionTime.ToString("yyyy-MM-dd hh:mm"),
                Type = handlingEvent.Type.ToString(),
                VoyageNumber = handlingEvent.Voyage.VoyageNumber.Value,
                IsExpected = handlingEvent.Cargo.Itinerary.IsExpectedActivity(handlingEvent.Activity),
                Description = GetHandlingEventDescription(handlingEvent),                
            };
        }

        private string GetHandlingEventDescription(HandlingEvent handlingEvent)
        {
            switch(handlingEvent.Type)
            {
                case HandlingActivityType.LOAD:
                    return String.Format("Loaded onto voyage {0} in {1}, at {2}.", 
                        handlingEvent.Voyage.VoyageNumber.Value,
                        handlingEvent.Location.Name,
                        handlingEvent.CompletionTime.ToShortTimeString());

                case HandlingActivityType.UNLOAD:
                    return String.Format("Unloaded off voyage {0} in {1}, at {2}.",
                        handlingEvent.Voyage.VoyageNumber.Value,
                        handlingEvent.Location.Name,
                        handlingEvent.CompletionTime.ToShortTimeString());

                case HandlingActivityType.RECEIVE:
                    return String.Format("Received in {0}, at {1}.",
                        handlingEvent.Location.Name,
                        handlingEvent.CompletionTime.ToShortTimeString());

                case HandlingActivityType.CLAIM:
                    return String.Format("Claimed in {0}, at {1}.",
                        handlingEvent.Location.Name,
                        handlingEvent.CompletionTime.ToShortTimeString());

                case HandlingActivityType.CUSTOMS:
                    return String.Format("Cleared customs in {0}, at {1}.",
                        handlingEvent.Location.Name,
                        handlingEvent.CompletionTime.ToShortTimeString());

                default:
                    return "Cargo has not yet been received.";
            }
        }
    }
}