using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

using DomainDrivenDelivery.Booking.Api;
using DomainDrivenDelivery.Booking.Web.Models;

namespace DomainDrivenDelivery.Booking.Web.Controllers
{
    public class BookingController : Controller
    {
        private readonly BookingServiceFacade _bookingServiceFacade;

        public BookingController(BookingServiceFacade bookingServiceFacade)
        {
            _bookingServiceFacade = bookingServiceFacade;
        }

        [HttpGet]
        public ActionResult CargoBookingForm()
        {
            var dtoList = _bookingServiceFacade.listShippingLocations();
            dtoList = dtoList.OrderBy(l => l.getUnLocode());
            
            return View(dtoList);
        }

        [HttpPost]
        public ActionResult BookCargo(CargoBookingCommand command)
        {
            var arrivalDeadline = DateTime.Parse(command.arrivalDeadline);
            var trackingId = _bookingServiceFacade.bookNewCargo(
                command.originUnlocode, 
                command.destinationUnlocode, 
                arrivalDeadline);

            return RedirectToAction("Show", new { trackingId });
        }

        [HttpGet]
        public ActionResult List()
        {
            var cargoList = _bookingServiceFacade.listAllCargos();
            return View(cargoList);
        }

        [HttpGet]
        public ActionResult Show(string id)
        {
            var dto = _bookingServiceFacade.loadCargoForRouting(id);
            return View(dto);
        }

        [HttpGet]
        public ActionResult SelectItinerary(string id)
        {
            var model = new SelectItineraryModel {
                Cargo = _bookingServiceFacade.loadCargoForRouting(id),
                RouteCandidates = _bookingServiceFacade.requestPossibleRoutesForCargo(id),
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult AssignItinerary(RouteAssignmentCommand command)
        {
            var legs = command.legs
                .Select(leg => new LegDTO(
                    leg.voyageNumber, 
                    leg.fromUnLocode, 
                    leg.toUnLocode, 
                    leg.fromDate, 
                    leg.toDate));

            var selectedRoute = new RouteCandidateDTO(legs);
            _bookingServiceFacade.assignCargoToRoute(command.trackingId, selectedRoute);

            return RedirectToAction("Show", new { command.trackingId });
        }

        [HttpGet]
        public ActionResult PickNewDestination(string id)
        {
            var model = new PickNewDestinationModel {
                Cargo = _bookingServiceFacade.loadCargoForRouting(id),
                Locations = _bookingServiceFacade.listShippingLocations(),
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult ChangeDestination(string trackingId, string unLocode)
        {
            _bookingServiceFacade.changeDestination(trackingId, unLocode);
            return RedirectToAction("Show", new { trackingId });
        }

        [HttpGet]
        public ActionResult VoyageDelayedForm()
        {
            var departures = new Dictionary<string, IEnumerable<string>>();
            var arrivals = new Dictionary<string, IEnumerable<string>>();
            var voyages = _bookingServiceFacade.listAllVoyages();

            foreach(var voyage in voyages)
            {
                var departureLocations = new List<string>();
                var arrivalLocations = new List<string>();

                foreach(var dto in voyage.getMovements())
                {
                    departureLocations.Add(dto.getDepartureLocation().getUnLocode());
                    arrivalLocations.Add(dto.getArrivalLocation().getUnLocode());
                }

                departures.Add(voyage.getVoyageNumber(), departureLocations);
                arrivals.Add(voyage.getVoyageNumber(), arrivalLocations);
            }

            var serializer = new JavaScriptSerializer();
            var model = new VoyageDelayedFormModel {
                DeparturesJson = new HtmlString(serializer.Serialize(departures)),
                ArrivalsJson = new HtmlString(serializer.Serialize(arrivals)),
                Voyages = voyages,
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult VoyageDelayed(VoyageDelayCommand command)
        {
            if(command.type == VoyageDelayCommand.DelayType.DEPT)
            {
                _bookingServiceFacade.departureDelayed(new VoyageDelayDTO(command.voyageNumber,
                    command.hours * 60));
            }

            else if(command.type == VoyageDelayCommand.DelayType.ARR)
            {
                _bookingServiceFacade.arrivalDelayed(new VoyageDelayDTO(command.voyageNumber,
                    command.hours * 60));
            }

            return RedirectToAction("List");
        }
    }
}